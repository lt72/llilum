//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Client
{
    using System.Net;
    using CoAP.Client;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.UdpTransport;


    internal class BasicUdpClient
    {
        //
        // State 
        // 
        
        private readonly CoAPClient m_client;

        //--//

        internal BasicUdpClient( IPEndPoint localEndPoint )
        {
            m_client = new CoAPClient( new Messaging( new UdpChannelFactory( ), localEndPoint ), new Statistics( ) ); 
        }

        internal MessageBuilder Connect( IPEndPoint intermediary, ServerCoAPUri uri )
        {
            return m_client.Connect( intermediary, uri );
        }

        internal CoAPMessage MakeRequest( CoAPMessageRaw request )
        {
            return m_client.SendReceive( request );
        }

        internal Statistics Statistics
        {
            get
            {
                return m_client.Statistics;
            }
        }
    }
}
