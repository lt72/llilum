

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;
    using CoAP.Server;
    using CoAP.UdpTransport;

    public class UdpTestProxyServer
    {
        //
        // State 
        // 

        private readonly ServerMessagingMock m_messagingMock;
        private readonly CoAPProxyServer     m_proxyServer;

        //--//

        public UdpTestProxyServer( IPEndPoint[ ] endPoints )
        {
            var messaging     = new Messaging( new UdpChannelFactory( ), endPoints[ 0 ] );
            var mockMessaging = new ServerMessagingMock( new AsyncMessagingProxy( messaging ) );

            messaging.OwnerMessaging = mockMessaging;

            var server = CoAPProxyServer.CreateProxyServer( endPoints, mockMessaging ); 
            
            m_messagingMock = mockMessaging;
            m_proxyServer   = server;
        }

        public void Start( )
        {
            m_proxyServer.Start( );
        }

        public void Stop( )
        {
            m_proxyServer.Stop( );
        }

        public void AddProvider( CoAPServerUri uri, ResourceProvider provider )
        {
            m_proxyServer.AddProvider( uri, provider );
        }

        public ServerMessagingMock MessagingMock
        {
            get
            {
                return m_messagingMock;
            }
        }

        public Statistics Statistics
        {
            get
            {
                return m_proxyServer.Statistics;
            }
        }
        
        public CoAPProxyServer Server
        {
            get
            {
                return m_proxyServer;
            }
        }
    }
}
