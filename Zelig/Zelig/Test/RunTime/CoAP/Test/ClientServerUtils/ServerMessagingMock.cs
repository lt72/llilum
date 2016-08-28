//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;

    public class ServerMessagingMock : AsyncMessagingProxy
    {
        private readonly AsyncMessagingProxy m_messaging;

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

        public event MessagingMockHandler OnIncomingMessageMock;

        public event MessagingMockHandler OnOutgoingMessageMock;

        public int ChangedMessagesCount
        {
            get; set;
        }

        public bool AnswerWithBadOption
        {
            get; set;
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

        public override void SendMessageAsync( CoAPMessageRaw msg )
        {
            var msgMock = OnOutgoingMessageMock;

            var messageCtx = MessageContext.WrapWithContext( msg );
            
            messageCtx.Source      = msg.Context.Source;
            messageCtx.Destination = msg.Context.Destination;

            var mockedArgs = new CoAPMessageEventArgs( messageCtx );

            bool? proceed  = msgMock?.Invoke( this, ref mockedArgs );

            if(proceed.HasValue ? proceed.Value : true)
            {
                m_messaging.SendMessageAsync( mockedArgs.MessageContext.Message );
            }
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

        private void MockMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var msgMock    = OnIncomingMessageMock;
            var msgHandler = m_messageHandler;

            var mockedArgs = args;
            bool? proceed  = msgMock?.Invoke( sender, ref mockedArgs );

            if(proceed.HasValue ? proceed.Value : true)
            {
                if(msgHandler != null)
                {
                    msgHandler.Invoke( sender, role, mockedArgs );
                }
            }
        }

        private void MockErrorHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            CoAPMessageHandler errHandler = m_errorHandler;

            if(errHandler != null)
            {
                errHandler.Invoke( sender, role,args );
            }
        }
    }
}