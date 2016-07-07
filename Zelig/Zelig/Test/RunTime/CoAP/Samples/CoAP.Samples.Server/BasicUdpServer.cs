//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Server
{
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

        internal BasicUdpServer( ServerCoAPUri uri )
        {
            var server = new CoAPServer( uri, new Messaging( new UdpChannelFactory( ), uri.EndPoints[0] ), new Statistics( ) );  
            
            server.AddProvider( "temperature/100", new TemperatureProvider( 100 ) );
            server.AddProvider( "temperature/200", new TemperatureProvider( 200 ) );

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
