

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Server;
    using CoAP.Stack;
    using CoAP.UdpTransport;
    using CoAP.Common.Diagnostics;


    public class UdpTestServer
    {
        //
        // State 
        // 

        private readonly ServerMessagingMock m_messagingMock;
        private readonly CoAPServer          m_server;
        private readonly IPEndPoint          m_endPoint;

        //--//

        public UdpTestServer( ServerCoAPUri uri )
        {
            var endPoint  = uri.EndPoints[0]; 
            var messaging = new ServerMessagingMock( new AsyncMessagingProxy( new Messaging( new UdpChannelFactory( ), endPoint ) ) );
            var server    = new CoAPServer( uri, messaging, new Statistics( ) );

            server.AddProvider( TestConstants.Resource__PingForAck_Immediate.Path, new PingProvider          ( ) );
            server.AddProvider( TestConstants.Resource__EchoQuery_Immediate .Path, new EchoProvider_Immediate( ) );
            server.AddProvider( TestConstants.Resource__EchoQuery_Delayed   .Path, new EchoProvider_Delayed  ( ) );

            m_messagingMock = messaging;
            m_server        = server;
            m_endPoint      = endPoint;
        }

        public void Start()
        {
            m_server.Start( ); 
        }

        public void Exit()
        {
            m_server.Stop( );
        }
        
        public IPEndPoint EndPoint
        {
            get
            {
                return m_endPoint;
            }
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
