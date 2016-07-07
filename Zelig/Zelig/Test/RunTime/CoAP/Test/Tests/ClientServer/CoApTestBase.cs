//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using System.Net;
    using Test.ClientServerUtils;
    using CoAP.Stack;
    using CoAP.Common;


    public class CoApTestBase : TestBase, ITestInterface
    {
        protected readonly IPEndPoint    DefaultClientEndPoint = new IPEndPoint   ( IPAddress.Loopback                      , 8081   );
        protected readonly ServerCoAPUri DefaultServeUri       = new ServerCoAPUri( Utils.EndPointsFromHostName( "localhost", 8080 ), "res" );

        protected readonly IPEndPoint    m_clientEndPoint;
        protected readonly ServerCoAPUri m_serverUri;
        protected          UdpTestClient m_client;
        protected          UdpTestServer m_server;

        //--//

        public CoApTestBase( )
        {
            m_clientEndPoint = DefaultClientEndPoint;
            m_serverUri      = DefaultServeUri;
        }


        public CoApTestBase( IPEndPoint clientEndPoint, ServerCoAPUri serverUri )
        {
            m_clientEndPoint = clientEndPoint;
            m_serverUri      = serverUri;
        }

        [SetUp]
        public virtual InitializeResult Initialize( )
        {
            Log.Comment( "*** Initialize client and server..." );
            Log.NewLine( );

            m_server = new UdpTestServer( DefaultServeUri );
            m_server.Start( );

            m_client = new UdpTestClient( m_clientEndPoint );

            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public virtual void CleanUp( )
        {
            Log.Comment( "*** Cleaning up after the tests" );
            Log.NewLine( );
            Log.NewLine( );

            m_client.Disconnect( );
            m_server.Exit( );
        }

        //--//

        protected void ClearStatistics( )
        {
            m_client.Statistics.Clear( );
            m_server.Statistics.Clear( );
        }

        protected static void TEST_DELAY( )
        {
            System.Threading.Thread.Sleep( 1000 ); 
        }
    }
}
