//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Server
{
    using System.Net;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;
    using CoAP.Server;
    using CoAP.Stack;
    using CoAP.UdpTransport;

    internal class BasicUdpServer
    {
        //
        // State 
        // 

        CoAPServer m_server;

        //--//

        internal BasicUdpServer( IPEndPoint[] endPoints )
        {
            var server = CoAPServer.CreateServer( endPoints, new Messaging( new UdpChannelFactory( ), endPoints[0] ) );

            server.AddProvider( new CoAPServerUri( new IPEndPoint( IPAddress.Loopback, 8080 ), "temperature/100" ), new TemperatureProvider( 100 ) );
            server.AddProvider( new CoAPServerUri( new IPEndPoint( IPAddress.Loopback, 8080 ), "temperature/200" ), new TemperatureProvider( 200 ) );

            m_server = server;
        }

        internal void Run()
        {
            m_server.Start( ); 
        }

        internal void Exit()
        {
            m_server.Stop( ); 
        }
    }
}
