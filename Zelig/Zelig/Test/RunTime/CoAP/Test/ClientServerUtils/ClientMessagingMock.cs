//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using System.Threading;
    using CoAP.Common.Diagnostics;


    public class ClientMessagingMock : AsyncMessaging
    {
        private readonly Messaging m_messaging;
        private          int       m_dropRequestCount;
        private          int       m_dropResponseCount;
        
        //--//

        public ClientMessagingMock( Messaging messaging ) : base ( messaging.ChannelFactory, messaging.LocalEndPoint )
        {
            m_messaging         = messaging;
            m_dropRequestCount  = 0;
            m_dropResponseCount = 0;

            //
            // Hijack messages from the actual messaging layer
            // 
            this.m_messaging.OnMessage += MockMessageHandler;
            this.m_messaging.OnError   += MockErrorHandler;
        }

        //
        // Helper methods
        // 
        
        public override void SendMessageAsync( CoAPMessageRaw msg, MessageContext messageCtx )
        {
            int dropped = this.DropRequestCount;

            if(Interlocked.Decrement( ref dropped ) >= 0)
            {
                Logger.Instance.LogWarning( "*** Request received, simulating drop..." );

                this.DropRequestCount = dropped;

                return;
            }

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

        public event MessagingMockHandler OnMessageMock;


        public int DropRequestCount
        {
            get
            {
                return m_dropRequestCount;
            }
            set
            {
                m_dropRequestCount = value;
            }
        }

        public int DropResponseCount
        {
            get
            {
                return m_dropResponseCount;
            }
            set
            {
                m_dropResponseCount = value;
            }
        }

        //--//

        private void MockMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var msgMock    = OnMessageMock;
            var msgHandler = m_messageHandler;

            var mockedMessage = args;
            bool? proceed = msgMock?.Invoke( sender, ref mockedMessage );

            if(proceed.HasValue ? proceed.Value : true)
            {
                if(msgHandler != null)
                {                    msgHandler.Invoke( sender, mockedMessage );
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
