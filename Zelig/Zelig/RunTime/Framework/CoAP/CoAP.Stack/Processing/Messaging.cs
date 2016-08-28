//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;

    public sealed class Messaging : AsyncMessaging
    {
        //
        // State 
        // 
        
        private readonly Queue<CoAPMessage>    m_incomingQueue;
        private readonly Queue<CoAPMessageRaw> m_outgoingQueue;
        private readonly AutoResetEvent        m_wakeup;
        private readonly MessageParser         m_parser;
        private          Thread                m_listener;
        private          Thread                m_dispatcher;
        private volatile bool                  m_running;
        private          ICoAPChannel          m_serverChannel;
        private readonly object                m_sync;
        
        //--//

        //
        // Constructors
        // 

        public Messaging( IChannelFactory channelFactory, IPEndPoint localEndPoint ) : base( channelFactory, localEndPoint )
        {
            m_incomingQueue  = new Queue<CoAPMessage>   ( ); 
            m_outgoingQueue  = new Queue<CoAPMessageRaw>( );
            m_wakeup         = new AutoResetEvent( false );
            m_parser         = new MessageParser( ); 
            m_sync           = new object( );

            this.OwnerMessaging = this;
        }

        //
        // Helper methods
        // 

        public override void SendMessageAsync( CoAPMessageRaw msg )
        {
            lock(m_sync)
            {
                m_outgoingQueue.Enqueue( msg );

                m_wakeup.Set( ); 
            }
        }
        
        public override void Start( )
        {
            if(m_running == false)
            {
                lock(m_sync)
                {
                    if(m_running == false)
                    {
                        m_running = true;

                        if(m_listener == null && m_dispatcher == null)
                        {
                            m_listener       = new Thread( this.ListenerLoop );
                            m_dispatcher     = new Thread( this.DispatcherLoop );
                            m_serverChannel  = this.ChannelFactory.Create( this.LocalEndPoint, true );

                            m_listener  .Start( );
                            m_dispatcher.Start( );
                        }
                        else
                        {
                            Debug.Assert( false ); 
                        }
                    }
                }
            }
        }

        public override void Stop( )
        {
            if(m_running == true)
            {
                Thread listener   = null;
                Thread dispatcher = null;

                lock(m_sync)
                {
                    if(m_running == true)
                    {
                        Debug.Assert( m_listener != null && m_dispatcher != null ); 

                        m_running = false;

                        listener   = m_listener;
                        dispatcher = m_dispatcher;

                        m_listener = m_dispatcher = null;

                        this.ChannelFactory.Retire( m_serverChannel ); 

                        m_incomingQueue.Clear( );
                        m_outgoingQueue.Clear( );
                      
                        m_wakeup.Set( );
                    }
                }
                        
                listener  .Join( ); 
                dispatcher.Join( );
            }
        }

        //
        // Access methods
        //

        public AsyncMessaging OwnerMessaging { get; set; }

        //--//

        private void ListenerLoop( )
        {
            EndPoint endPoint = new IPEndPoint( IPAddress.Any, 0 );
            var channel       = m_serverChannel;

            try
            {
                var buffer = new byte[ 256 ];

                while(m_running)
                {

                    int received = 0;
                    try
                    {
                        received = channel.Receive( buffer, 0, buffer.Length, ref endPoint );

                        if( received == 0 )
                        {
                            //
                            // Shutting down
                            //

                            break;
                        }
                    }
                    catch(SocketException)
                    {
                        break;
                    }

                    //
                    // Update length and content to match the number of bytes received
                    // 
                    var msg        = CoAPMessage.FromBuffer( new byte[ received ] );
                    var messageCtx = MessageContext.WrapWithContext( msg );

                    Buffer.BlockCopy( buffer, 0, msg.Buffer, 0, received );

                    messageCtx.Source = (IPEndPoint)endPoint;
                    
                    lock(m_sync)
                    {
                        if(m_running == false)
                        {
                            break;
                        }

                        m_incomingQueue.Enqueue( msg );

                        m_wakeup.Set( );
                    }
                }
            }
            finally
            {
                channel.Close( ); 
            }
        }
        
        private void DispatcherLoop( )
        {
            while(m_running)
            {
                try
                {

                    m_wakeup.WaitOne( );

                    //
                    // Dispatch all events in the queue, and protect from a spourious wake up 
                    // that could happen by messeges queued while processing the queues. 
                    //
                    int messages = 0;

                    lock(m_sync)
                    {
                        messages = m_incomingQueue.Count + m_outgoingQueue.Count;
                    }

                    while(--messages >= 0)
                    {
                        CoAPMessage    msgIn  = null;
                        CoAPMessageRaw msgOut = null;

                        //
                        // Dequeue one event from each queue
                        //
                        lock (m_sync)
                        {
                            if(m_running == false)
                            {
                                break;
                            }

                            if(m_incomingQueue.Count > 0)
                            {
                                msgIn = m_incomingQueue.Dequeue( );
                            }

                            if(m_outgoingQueue.Count > 0)
                            {
                                msgOut = m_outgoingQueue.Dequeue( );
                            }
                        }

                        if(m_running == false)
                        {
                            break;
                        }

                        //
                        // Here we may be caught mid-way on a shutdown, so do not do anything too dangerous
                        //
                        if(msgIn != null)
                        {
                            Debug.Assert( msgIn.Buffer != null          );
                            Debug.Assert( msgIn is CoAPMessage == true  );
                            //
                            // Parse message, be careful not to use the parser for any other message until done
                            // 
                            HandlerRole role = HandlerRole.Local;
                            try
                            {

                                if(m_parser.ParseAndComputeDestination( msgIn.Buffer, this.LocalEndPoint, ref msgIn ))
                                {
                                    var msgHandler = m_messageHandler;

                                    msgHandler?.Invoke( this, role, new CoAPMessageEventArgs(msgIn.Context ) );
                                }
                                else
                                {
                                    var errHandler = m_errorHandler;

                                    errHandler?.Invoke( this, role, new CoAPMessageEventArgs( msgIn.Context ) );
                                }
                            }
                            catch(Exception ex)
                            {
                                var errHandler = m_errorHandler;

                                Logger.Instance.LogError( $"Caught exception: {ex}" );

                                errHandler?.Invoke( this, role, new CoAPMessageEventArgs( msgIn.Context ) );
                            }
                        }

                        if(msgOut != null)
                        {
                            Debug.Assert( msgOut.Buffer != null          );
                            Debug.Assert( msgOut is CoAPMessage == false );

                            var destination = msgOut.Context.Source;

                            m_serverChannel.Send( msgOut.Buffer, 0, msgOut.Buffer.Length, destination );
                        }
                    }

                }
                catch(SocketException)
                {
                    //
                    // Socket may have been closed after pulling the message from the queue, 
                    // just bail out
                    //
                    break;
                }
                catch
                {
                    // TODO: what logging
                }
            }
        }
    }
}
