

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;
    using CoAP.Server;
    using CoAP.UdpTransport;
    using System;

    public class UdpTestServer
    {
        //
        // State 
        // 

        private readonly ServerMessagingMock m_messagingMock;
        private readonly CoAPServer          m_server;

        //--//

        public UdpTestServer( IPEndPoint[] endPoints )
        {
            var messaging     = new Messaging( new UdpChannelFactory( ), endPoints[ 0 ] );
            var mockMessaging = new ServerMessagingMock( new AsyncMessagingProxy( messaging ) );

            messaging.OwnerMessaging = mockMessaging;

            var server = CoAPServer.CreateServer( endPoints, mockMessaging ); 

            m_messagingMock = mockMessaging;
            m_server        = server;
        }

        public void Start()
        {
            m_server.Start( ); 
        }

        public void Stop()
        {
            m_server.Stop( );
        }
        
        public void AddProvider( CoAPServerUri uri, ResourceProvider provider )
        {
            m_server.AddProvider( uri, provider );
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
                return m_server.Statistics;
            }
        }

        //--//

    }
}
