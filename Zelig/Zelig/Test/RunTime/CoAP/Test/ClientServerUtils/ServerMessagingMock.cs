//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Server;
    using System;

    public class ServerMessagingMock : AsyncMessagingProxy
    {
        private readonly AsyncMessagingProxy m_messaging;
        private          int                 m_changedMessagesCount;

        //--//

        public ServerMessagingMock( AsyncMessagingProxy messaging ) : base( messaging )
        {
            this.m_messaging = messaging;

            //
            // Hijack messages from the actual messaging layer
            // 
            this.m_messaging.OnMessage += MockMessageHandler;
            this.m_messaging.OnError   += MockErrorHandler;
        }

        //
        // Helper methods
        //

        public event MessagingMockHandler OnMessageMock;
       
        public int ChangedMessagesCount
        {
            get
            {
                return m_changedMessagesCount;
            }
            set
            {
                m_changedMessagesCount = value;
            }
        }

        public override event CoAPProxyMessageHandler OnProxyMessage
        {
            add
            {
                m_messaging.OnProxyMessage += value;
            }
            remove
            {
                m_messaging.OnProxyMessage -= value;
            }
        }

        public override void SendMessageAsync( CoAPMessageRaw msg, MessageContext messageCtx )
        {
            m_messaging.SendMessageAsync( msg, messageCtx );
        }

        public override void Start( )
        {
            m_messaging.Start( );
        }

        public override void Stop( )
        {
            m_messaging.Stop( );
        }

        //--//

        private void MockMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var msgMock    = OnMessageMock;
            var msgHandler = m_messageHandler;

            var mockedArgs = args;
            bool? proceed  = msgMock?.Invoke( sender, ref mockedArgs );

            if(proceed.HasValue ? proceed.Value : true)
            {
                if(msgHandler != null)
                {
                    msgHandler.Invoke( sender, mockedArgs );
                }
            }
        }

        private void MockErrorHandler( object sender, CoAPMessageEventArgs args )
        {
            CoAPMessageHandler errHandler = m_errorHandler;

            if(errHandler != null)
            {
                errHandler.Invoke( sender, args );
            }
        }
    }
}