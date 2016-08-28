//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;
    using System.Threading;
    using CoAP.Common.Diagnostics;

    public class ClientMessagingMock : AsyncMessaging
    {
        private readonly Messaging m_messaging;
        
        //--//

        public ClientMessagingMock( Messaging messaging ) : base ( messaging.ChannelFactory, messaging.LocalEndPoint )
        {
            m_messaging = messaging;

            //
            // Hijack messages from the actual messaging layer
            // 
            this.m_messaging.OnMessage += MockMessageHandler;
            this.m_messaging.OnError   += MockErrorHandler;
        }

        //
        // Helper methods
        // 

        public override void SendMessageAsync( CoAPMessageRaw msg )
        {
            int dropped = this.DropRequestCount;

            if(Interlocked.Decrement( ref dropped ) >= 0)
            {
                Logger.Instance.LogWarning( "*** Request received, simulating drop..." );

                this.DropRequestCount = dropped;

                return;
            }

            m_messaging.SendMessageAsync( msg );
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
            get; set;
        }

        public int DropResponseCount
        {
            get; set;
        }

        //--//

        private void MockMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var msgMock    = OnMessageMock;
            var msgHandler = m_messageHandler;

            var mockedMessage = args;
            bool? proceed = msgMock?.Invoke( sender, ref mockedMessage );

            if(proceed.HasValue ? proceed.Value : true)
            {
                if(msgHandler != null)
                {
                    msgHandler.Invoke( sender, role, mockedMessage );
                }
            }
        }

        private void MockErrorHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            CoAPMessageHandler errHandler = m_errorHandler;

            if(errHandler != null)
            {
                errHandler.Invoke( sender, role, args );
            }
        }
    }
}
