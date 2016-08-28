//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Net;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;

    //--//

    public class AsyncMessagingProxy : AsyncMessaging
    {

        public delegate HandlerRole CoAPProxyMessageHandler( object sender, ref CoAPMessageEventArgs args );

        //--//

        //
        // State 
        // 
        private readonly AsyncMessaging          m_messaging;
        private          CoAPProxyMessageHandler m_proxyHandler;


        //--//

        public AsyncMessagingProxy( AsyncMessaging messaging ) : base( messaging.ChannelFactory, messaging.LocalEndPoint )
        {
            m_messaging = messaging;
        }

        //
        // Helper methods
        // 

        public virtual event CoAPProxyMessageHandler OnProxyMessage
        {
            add
            {
                //
                // There can bee only one to allow filterings to work!
                // 
                if(m_proxyHandler == null)
                {
                    m_proxyHandler += value;

                    return; 
                }

                throw new InvalidOperationException( ); 
            }
            remove
            {
                m_proxyHandler -= value;
            }
        }

        public override void SendMessageAsync( CoAPMessageRaw msg )
        {
            m_messaging.SendMessageAsync( msg );
        }

        public override void Start( )
        {
            //
            // Hijack messages from the actual messaging layer
            // 
            m_messaging.OnMessage += Proxy_IncomingMessageHandler;
            m_messaging.OnError   += Proxy_ErrorMessageHandler;

            m_messaging.Start( );
        }

        public override void Stop( )
        {
            m_messaging.Stop( );

            m_messaging.OnMessage -= Proxy_IncomingMessageHandler;
            m_messaging.OnError   -= Proxy_ErrorMessageHandler;
        }

        //--//

        private void Proxy_IncomingMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var proxyHandler   = m_proxyHandler;
            var messageHandler = m_messageHandler;

            var proxiedArgs  = args;

            role = HandlerRole.Local;

            if(proxyHandler != null)
            {
                role = proxyHandler.Invoke( sender, ref proxiedArgs );
            }
            
            if(role != HandlerRole.Misrouted)
            {
                if(messageHandler != null)
                {
                    messageHandler.Invoke( sender, role, proxiedArgs );
                }
            }
            else
            {
                args.MessageContext.ResponseCode = CoAPMessage.ServerError_WithDetail( CoAPMessage.Detail_ServerError.ProxyingNotSupported );

                Proxy_ErrorMessageHandler( sender, role, args ); 
            }
        }

        private void Proxy_ErrorMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var errHandler = m_errorHandler;

            if(errHandler != null)
            {
                errHandler.Invoke( sender, role, args );
            }
        }
    }
}
