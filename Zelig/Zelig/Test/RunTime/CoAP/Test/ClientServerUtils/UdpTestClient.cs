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
        private readonly Statistics          m_stats;

        //--//

        public UdpTestClient( IPEndPoint localEndPoint )
        {
            var messaging = new ClientMessagingMock( new Messaging( new UdpChannelFactory( ), localEndPoint ) );

            m_stats         = new Statistics( ); 
            m_client        = new CoAPClient( messaging, m_stats );
            m_messagingMock = messaging;
        }

        public MessageBuilder Connect( IPEndPoint intermediary, ServerCoAPUri uri )
        {
            return m_client.Connect( intermediary, uri );
        }

        public void Disconnect( )
        {
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
