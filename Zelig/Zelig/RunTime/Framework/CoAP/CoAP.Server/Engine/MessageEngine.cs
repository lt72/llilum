//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Collections.Generic;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;

    public class MessageEngine
    {
        //
        // State 
        // 
        private readonly List<MessageContext>                 m_oustanding;
        private readonly Dictionary<ushort, MessageProcessor> m_awaitingAck;
        private readonly IServer                              m_owner;
        private readonly AsyncMessaging                       m_messaging;
        private readonly object                               m_sync;

        //--//

        //
        // Constructors  
        // 

        public MessageEngine( CoAPServer owner, AsyncMessaging messaging )
        {
            m_oustanding  = new List<MessageContext>( ); 
            m_awaitingAck = new Dictionary<ushort, MessageProcessor>( );
            m_owner       = owner;
            m_messaging   = messaging;
            m_sync        = new object( ); 
        }

        //
        // Helper Methods
        //
        // 

        public void Start( )
        {
            m_messaging.OnMessage += this.Messaging_IncomingMessageHandler;
            m_messaging.OnError   += this.Messaging_ErrorMessageHandler;

            if(m_messaging is AsyncMessagingProxy)
            {

                ((AsyncMessagingProxy)m_messaging).OnProxyMessage += this.ProxyMessageHandler_PassThrough;
            }

            m_messaging.Start( );
        }

        public void Stop( )
        {
            m_messaging.Stop( );

            if(m_messaging is AsyncMessagingProxy)
            {

                ((AsyncMessagingProxy)m_messaging).OnProxyMessage -= this.ProxyMessageHandler_PassThrough;
            }

            m_messaging.OnError   -= this.Messaging_ErrorMessageHandler;
            m_messaging.OnMessage -= this.Messaging_IncomingMessageHandler;
        }

        //
        // Access Methods
        //

        public IServer Owner
        {
            get
            {
                return m_owner;
            }
        }

        public bool IsAckPending( ushort messageId )
        {
            lock (m_sync)
            {
                return m_awaitingAck.ContainsKey( messageId );
            }
        }

        public void RegisterAckPending( ushort messageId, MessageProcessor proc )
        {
            lock (m_sync)
            {
                m_awaitingAck.Add( messageId, proc );
            }
        }

        public bool TryRemoveAckPending( ushort messageId, out MessageProcessor proc )
        {
            bool fRemoved = false;

            lock(m_sync)
            {
                if(m_awaitingAck.TryGetValue( messageId, out proc ))
                {
                    fRemoved = m_awaitingAck.Remove( messageId );
                }
            }

            return fRemoved;
        }

        //--//

        internal void Register( MessageContext node )
        {
            lock (m_sync)
            {
                m_oustanding.Add( node );
            }
        }

        internal void Deregister( MessageContext node )
        {
            lock (m_sync)
            {
                m_oustanding.Remove( node );
            }
        }

        private void Messaging_IncomingMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var processor = AsyncMessageProcessor.CreateMessageProcessor( 
                args.MessageContext, this
                );

            Register( processor.MessageContext );

            processor.Process( );
        }

        private void Messaging_ErrorMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var messageCtx = args.MessageContext;
            var error      = args.MessageContext.Error;

            AsyncMessageProcessor processor = null;

            if(error == CoAPMessageRaw.Error.Parsing__OptionError)
            {
                processor = AsyncMessageProcessor.CreateOptionsErrorProcessor( messageCtx, this );
            }
            else
            {
                processor = AsyncMessageProcessor.CreateErrorProcessor( messageCtx, this );
            }

            Register( processor.MessageContext );

            processor.Process( );
        }
        
        private bool ProxyMessageHandler_PassThrough( object sender, ref CoAPMessageEventArgs args )
        {
            var messageCtx = args.MessageContext;

            var targets     = this.Owner.EndPoints;
            var destination = messageCtx.Destination;

            for(int i = 0; i < targets.Length; i++)
            {
                if(targets[ i ].Equals( destination ))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
