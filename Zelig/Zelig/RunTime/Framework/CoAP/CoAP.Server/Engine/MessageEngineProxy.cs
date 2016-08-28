//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Net;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;

    internal sealed class MessageEngineProxy : MessageEngine
    {
        //
        // State 
        // 

        private IPEndPoint[] m_proxyEndPoints;

        //--//

        //
        // Constructors  
        // 

        internal MessageEngineProxy( IPEndPoint[ ] originEndPoints, AsyncMessaging messaging ) : base ( originEndPoints, messaging )
        {
            m_proxyEndPoints = new IPEndPoint[ 0 ];
        }

        //
        // Helper Methods
        //
        // 

        internal override void Start( )
        {
            var messagingProxy = this.Messaging as AsyncMessagingProxy;

            if(messagingProxy != null)
            {
                messagingProxy.OnProxyMessage += ProxyMessageHandler_Admit;
            }

            base.Start( );

        }

        internal override void Stop( )
        {
            base.Stop( ); 

            var messagingProxy = this.Messaging as AsyncMessagingProxy;
            if(messagingProxy != null)
            {
                messagingProxy.OnProxyMessage -= ProxyMessageHandler_Admit;
            }
        }

        //
        // Access Methods
        //

        internal IPEndPoint[ ] ProxyEndPoints
        {
            get
            {
                return m_proxyEndPoints;
            }
            set
            {
                m_proxyEndPoints = value;
            }
        }

        //--//

        private HandlerRole ProxyMessageHandler_Admit( object sender, ref CoAPMessageEventArgs args )
        {
            var destination = args.MessageContext.Destination;
            var source      = args.MessageContext.Source;

            //
            // Message must be directed to either a local end point or a proxied end point.
            // Check the proxy case first, where we need to check both source and destination. 
            // 

            if(this.ProxyEndPoints != null)
            {
                for(int i = 0; i < this.ProxyEndPoints.Length; i++)
                {
                    if(this.ProxyEndPoints[ i ].Equals( destination ) || this.ProxyEndPoints[ i ].Equals( source ))
                    {
                        return HandlerRole.Proxy;
                    }
                }
            }

            if(this.OriginEndPoints != null)
            {
                for(int i = 0; i < this.OriginEndPoints.Length; i++)
                {
                    if(this.OriginEndPoints[ i ].Equals( destination ))
                    {
                        return HandlerRole.Local;
                    }
                }
            }

            return HandlerRole.Misrouted;
        }
    }
}
