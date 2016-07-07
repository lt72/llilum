//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;

    //--//
    
    public delegate bool CoAPProxyMessageHandler( object sender, ref CoAPMessageEventArgs args );

    //--//

    public class AsyncMessagingProxy : AsyncMessaging
    {
        //
        // State 
        // 
        private readonly AsyncMessaging          m_messaging;
        private          CoAPProxyMessageHandler m_proxyHandler;


        //--//

        public AsyncMessagingProxy( AsyncMessaging messaging ) : base( messaging.ChannelFactory, messaging.LocalEndPoint )
        {
            this.m_messaging = messaging;
        }

        //
        // Helper methods
        // 

        public virtual event CoAPProxyMessageHandler OnProxyMessage
        {
            add
            {
                m_proxyHandler += value;
            }
            remove
            {
                m_proxyHandler -= value;
            }
        }

        public override void SendMessageAsync( CoAPMessageRaw msg, MessageContext messageCtx )
        {
            m_messaging.SendMessageAsync( msg, messageCtx );
        }

        public override void Start( )
        {
            //
            // Hijack messages from the actual messaging layer
            // 
            this.m_messaging.OnMessage += Proxy_IncomingMessageHandler;
            this.m_messaging.OnError   += Proxy_ErrorMessageHandler;

            m_messaging.Start( );
        }

        public override void Stop( )
        {
            m_messaging.Stop( );

            this.m_messaging.OnMessage -= Proxy_IncomingMessageHandler;
            this.m_messaging.OnError   -= Proxy_ErrorMessageHandler;
        }

        //--//

        private void Proxy_IncomingMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var msgProxy   = this.m_proxyHandler;
            var msgHandler = m_messageHandler;

            var proxiedArgs = args;
            bool? proceed   = msgProxy?.Invoke( sender, ref proxiedArgs );

            if(proceed.HasValue ? proceed.Value : true)
            {
                if(msgHandler != null)
                {
                    msgHandler.Invoke( sender, proxiedArgs );
                }
            }
            else
            {
                args.MessageContext.ResponseCode = CoAPMessage.ServerError_WithDetail( CoAPMessage.Detail_ServerError.ProxyingNotSupported );

                Proxy_ErrorMessageHandler( sender, args ); 
            }
        }

        private void Proxy_ErrorMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var errHandler = m_errorHandler;

            if(errHandler != null)
            {
                errHandler.Invoke( sender, args );
            }
        }
    }
}
