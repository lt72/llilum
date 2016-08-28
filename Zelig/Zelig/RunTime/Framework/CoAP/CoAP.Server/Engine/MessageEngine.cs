//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;

    internal class MessageEngine
    {

        class RequestComparer : IEqualityComparer<MessageContext>
        {
            public bool Equals( MessageContext x, MessageContext y )
            {
                return  x.Message.MessageId == y.Message.MessageId && 
                        x.Source.Equals( y.Source )                 ; 
            }

            public int GetHashCode( MessageContext ctx )
            {
                return ctx.Source.GetHashCode( ) ^ ctx.Message.MessageId;
            }
        }
        
        class AckComparer : IEqualityComparer<MessageContext>
        {
            public bool Equals( MessageContext ctxA, MessageContext ctxB )
            {
                var messageIdA = (ctxA.ResponseAwaitingAck != null ? ctxA.ResponseAwaitingAck.MessageId : ctxA.Message.MessageId);
                var messageIdB = (ctxB.ResponseAwaitingAck != null ? ctxB.ResponseAwaitingAck.MessageId : ctxB.Message.MessageId);

                return messageIdA == messageIdB          &&
                       ctxA.Source.Equals( ctxB.Source )  ;
            }

            public int GetHashCode( MessageContext ctx )
            {
                return  (ctx.ResponseAwaitingAck != null ? ctx.ResponseAwaitingAck.MessageId : ctx.Message.MessageId) ^
                         ctx.Source.GetHashCode( )                                                                    ;
            }
        }

        //--//

        class FulfilledRequestEntry
        {
            public readonly CoAPMessageRaw Response;
            public readonly DateTime       FullfilmentTime;

            internal FulfilledRequestEntry( CoAPMessageRaw response )
            {
                this.Response        = response;
                this.FullfilmentTime = DateTime.Now;
            }
        }

        //--//
        //--//
        //--//

        //
        // State 
        // 

        private readonly Dictionary<MessageContext, FulfilledRequestEntry> m_oustandingLocalRequests;
        private readonly Dictionary<MessageContext, MessageProcessor     > m_awaitingAck;
        private readonly IPEndPoint[]                                      m_originEndPoints;
        private readonly AsyncMessaging                                    m_messaging;
        private CoAPServer                                                 m_owner;
        private readonly object                                            m_sync;

        //--//

        //
        // Constructors  
        // 

        internal MessageEngine( IPEndPoint[ ] originEndPoints, AsyncMessaging messaging )
        {
            m_oustandingLocalRequests = new Dictionary<MessageContext, FulfilledRequestEntry>( new RequestComparer( ) );
            m_awaitingAck             = new Dictionary<MessageContext, MessageProcessor     >( new AckComparer    ( ) );
            m_originEndPoints         = originEndPoints;
            m_messaging               = messaging;
            m_sync                    = new object( ); 
        }

        //
        // Helper Methods
        //
        // 

        internal virtual void Start( )
        {
            m_messaging.OnMessage += Messaging_IncomingMessageHandler;
            m_messaging.OnError   += Messaging_ErrorMessageHandler;

            m_messaging.Start( );
        }

        internal virtual void Stop( )
        {
            m_messaging.Stop( );

            m_messaging.OnError   -= Messaging_ErrorMessageHandler;
            m_messaging.OnMessage -= Messaging_IncomingMessageHandler;
        }

        internal void SendMessageAsync( CoAPMessageRaw msg )
        {
            //
            // Update tracked responses in case we need to serve them again, e.g. when an ACK gets lost or such...
            //
            MarkLocalRequestFulfilled( msg );

            SendMessageAsyncDirect( msg );
        }

        internal void SendMessageAsyncDirect( CoAPMessageRaw msg )
        {
            m_messaging.SendMessageAsync( msg );
        }

        internal bool IsAckPending( MessageContext messageCtx )
        {
            lock(m_sync)
            {
                return m_awaitingAck.ContainsKey( messageCtx );
            }
        }

        internal bool RegisterAckPending( MessageContext messageCtx, MessageProcessor proc )
        {
            lock(m_sync)
            {
                if(m_awaitingAck.ContainsKey( messageCtx ) == false)
                {
                    m_awaitingAck.Add( messageCtx, proc );

                    return true;
                }
            }

            return false;
        }

        internal bool TryRemoveAckPending( MessageContext messageCtx, out MessageProcessor proc )
        {
            bool fRemoved = false;

            lock(m_sync)
            {
                if(m_awaitingAck.TryGetValue( messageCtx, out proc ))
                {
                    fRemoved = m_awaitingAck.Remove( messageCtx );
                }
            }

            return fRemoved;
        }

        internal void SetOwner( CoAPServer owner )
        {
            m_owner = owner;
        }

        //
        // Access Methods
        //

        public AsyncMessaging Messaging
        {
            get
            {
                return m_messaging;
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return m_messaging.LocalEndPoint;
            }
        }

        public IPEndPoint[ ] OriginEndPoints
        {
            get
            {
                return m_originEndPoints;
            }
        }

        public CoAPServer Owner
        {
            get
            {
                return m_owner;
            }
        }

        //--//

        internal void RegisterLocalRequest( MessageContext ctx )
        {
            lock(m_sync)
            {
                m_oustandingLocalRequests.Add( ctx, null );
            }
        }

        private void MarkLocalRequestFulfilled( CoAPMessageRaw msg )
        {
            FulfilledRequestEntry entry = null;
            if(m_oustandingLocalRequests.TryGetValue( msg.Context, out entry ))
            {
                if(entry == null)
                {
                    m_oustandingLocalRequests[ msg.Context ] = new FulfilledRequestEntry( msg );
                }
            }
        }

        internal void DeregisterLocalRequest( MessageContext ctx )
        {
            //
            // TODO: do not remove the request until the EXCHANGE_LIFETIME window elapses. 
            // Note that for Confirmable responses this lookup will overlap with the lookup
            // for responses awaiting ACK. See https://github.com/lt72/CoAP-pr/issues/66. 
            //
            Debug.Assert( m_oustandingLocalRequests.ContainsKey( ctx ) ); 

            lock(m_sync)
            {
                m_oustandingLocalRequests.Remove( ctx );
            }
        }

        internal bool CheckLocalRegistrar( MessageContext ctx, out CoAPMessageRaw msg )
        {
            msg = null;

            lock(m_sync)
            {
                FulfilledRequestEntry entry = null;

                if(m_oustandingLocalRequests.TryGetValue( ctx , out entry ))
                {
                    if(entry != null)
                    {
                        msg = entry.Response;
                    }

                    return true;
                }
            }

            return false;
        }
       
        private void Messaging_IncomingMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var messageCtx          = args.MessageContext;
            CoAPMessageRaw response = null;

            //
            // Discard requests for proxy endpoints
            // 
            if(role == HandlerRole.Local)
            {
                bool fRegistered = CheckLocalRegistrar( messageCtx, out response );

                if(messageCtx.Message.IsAck || messageCtx.Message.IsReset || fRegistered == false)
                {
                    //
                    // Local requests should be processed only the first time they are received during the EXCHANGE_LIFETIME interval.
                    //

                    var processor = AsyncMessageProcessor.CreateMessageProcessor( messageCtx, this );

                    if(fRegistered == false)
                    {
                        RegisterLocalRequest( messageCtx );
                    }

                    processor.Process( );

                    return;
                }
                else
                {
                    //
                    // This is the case where a request is received for a message we already sent. 
                    // Replay the response without processing. 
                    //

                    if(response != null)
                    {
                        // TODO: should we just send it? SendMessageAsyncDirect( response );
                        AsyncMessageProcessor.CreateReplayResponseProcessor( messageCtx, response, this ).Process( );
                    }
                }
            }
        }

        private void Messaging_ErrorMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var messageCtx = args.MessageContext;

            //
            // Process requests for proxy endpoints as well as local requests.
            //
            CoAPMessageRaw response = null;
            if(CheckLocalRegistrar( messageCtx, out response ) == false)
            {
                //
                // Local requests should be processed only the first time they are received during the EXCHANGE_LIFETIME interval.
                //

                var error = messageCtx.ProtocolError;

                AsyncMessageProcessor processor = null;

                if(error == CoAPMessageRaw.Error.Parsing__OptionError)
                {
                    processor = AsyncMessageProcessor.CreateOptionsErrorProcessor( messageCtx, this );
                }
                else
                {
                    processor = AsyncMessageProcessor.CreateErrorProcessor( messageCtx, this );
                }

                RegisterLocalRequest( messageCtx );

                processor.Process( );
            }
            else
            {
                //
                // This is the case where a request is received for a message we already sent. 
                // Replay the response without processing. 
                //

                if(response != null)
                {
                    // TODO: should we just send it? SendMessageAsyncDirect( response );
                    AsyncMessageProcessor.CreateReplayResponseProcessor( messageCtx, response, this ).Process( );
                }
            }
        }
    }
}
