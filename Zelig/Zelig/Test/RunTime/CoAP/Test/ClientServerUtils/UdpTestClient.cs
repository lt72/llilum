//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Stack;
    using CoAP.Client;
    using CoAP.UdpTransport;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public class UdpTestClient
    {
        //
        // State 
        // 

        private readonly ClientMessagingMock m_messagingMock;
        private readonly CoAPClient          m_client;

        //--//

        public UdpTestClient( IPEndPoint localEndPoint )
        {
            var messaging     = new Messaging( new UdpChannelFactory( ), localEndPoint );
            var mockMessaging = new ClientMessagingMock( messaging );
            
            messaging.OwnerMessaging = mockMessaging;

            var client = new CoAPClient( mockMessaging, new Statistics( ) );
            
            m_client        = client;
            m_messagingMock = mockMessaging;
        }

        public MessageBuilder Connect( IPEndPoint intermediary, CoAPServerUri uri )
        {
            var builder = m_client.Connect( intermediary, uri );

            m_client.Start( );

            return builder;
        }

        public void Disconnect( )
        {
            m_client.Stop      ( );
            m_client.Disconnect( );
        }

        public CoAPMessage MakeRequest( CoAPMessageRaw request )
        {
            return m_client.SendReceive( request );
        }

        public ClientMessagingMock MessagingMock
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
                return m_client.Statistics;
            }
        }
    }
}
