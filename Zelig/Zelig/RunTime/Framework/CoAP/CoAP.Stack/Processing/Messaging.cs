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
    using CoAP.Common;
    using CoAP.Common.Diagnostics;


    public class Messaging : AsyncMessaging
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
        }

        //
        // Helper methods
        // 

        public override void SendMessageAsync( CoAPMessageRaw msg, MessageContext messageCtx )
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

        //--//

        private void ListenerLoop( )
        {
            EndPoint endPoint = new IPEndPoint( IPAddress.Any, 0 );
            var channel       = m_serverChannel;

            try
            {
                var temp = new byte[ 256 ];

                while(m_running)
                {

                    int received = 0;
                    try
                    {
                        received = channel.Receive( temp, 0, temp.Length, ref endPoint );

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

                    lock(m_sync)
                    {
                        if(m_running == false)
                        {
                            break;
                        }

                        //
                        // Update length and content to match the number of bytes received
                        // 
                        var msg        = CoAPMessage.FromBuffer( new byte[ received ] );
                        var messageCtx = new MessageContext( msg );

                        Buffer.BlockCopy( temp, 0, msg.Buffer, 0, msg.Buffer.Length );

                        messageCtx.Source  = (IPEndPoint)endPoint;
                        messageCtx.Channel = channel;

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
                    CoAPMessage    msgIn  = null;
                    CoAPMessageRaw msgOut = null;

                    m_wakeup.WaitOne( );

                    //
                    // Dispatch all events in the queue, and protect from a spourious wake up 
                    // that could happen by messeges queued while processing the queues. 
                    //
                    int messages = m_incomingQueue.Count + m_outgoingQueue.Count;

                    while(--messages >= 0)
                    {
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
                                msgIn  = m_incomingQueue.Dequeue( );
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
                            //
                            // Parse message, be careful not to use the parser for any other message until done
                            // 
                            try
                            {
                                if(m_parser.Parse( msgIn, this.LocalEndPoint ))
                                {
                                    var msgHandler = m_messageHandler;

                                    msgHandler?.Invoke( this, new CoAPMessageEventArgs { MessageContext = msgIn.Context } );
                                }
                                else
                                {
                                    var errHandler = m_errorHandler;

                                    msgIn.Context.Error = CoAPMessageRaw.Error.Parsing__OptionError;

                                    errHandler?.Invoke( this, new CoAPMessageEventArgs { MessageContext = msgIn.Context } );
                                }
                            }
                            catch(Exception ex)
                            {
                                var errHandler = m_errorHandler;

                                Logger.Instance.LogError( $"Caught exception: {ex}" );

                                errHandler?.Invoke( this, new CoAPMessageEventArgs { MessageContext = msgIn.Context } );
                            }
                        }

                        if(msgOut != null)
                        {
                            m_serverChannel.Send( msgOut.Buffer, 0, msgOut.Buffer.Length, msgOut.Context.Source );
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
