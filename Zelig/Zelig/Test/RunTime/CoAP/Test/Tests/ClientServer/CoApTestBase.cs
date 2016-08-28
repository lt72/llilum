//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using System.Net;
    using Test.ClientServerUtils;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Server;

    public class CoApTestBase : TestBase, ITestInterface
    {
        protected UdpTestClient               m_client;
        protected UdpTestProxyServer          m_localProxyServer;
        protected UdpTestServer               m_originServer;
        protected EchoProviderProxy_Immediate m_echoProviderProxy_Immediate;
        protected EchoProviderProxy_Delayed   m_echoProviderProxy_Delayed;

        //--//

        public CoApTestBase( )
        {
        }

        [SetUp]
        public virtual InitializeResult Initialize( )
        {
            Log.Comment( "*** Initialize client and server..." );
            Log.NewLine( );

            m_localProxyServer = new UdpTestProxyServer( TestConstants.AllRootLocalOriginEndPoints  );
            m_originServer     = new UdpTestServer     ( TestConstants.AllRootRemoteOriginEndPoints );

            //
            // Local providers for local origin server
            // 
            m_localProxyServer.AddProvider( TestConstants.Resource__PingForAck_Immediate, new PingProvider          ( ) );
            m_localProxyServer.AddProvider( TestConstants.Resource__EchoQuery_Immediate , new EchoProvider_Immediate( ) );
            m_localProxyServer.AddProvider( TestConstants.Resource__EchoQuery_Delayed   , new EchoProvider_Delayed  ( ) );

            //
            // Proxies for remote providers
            //
            m_echoProviderProxy_Immediate = new EchoProviderProxy_Immediate( TestConstants.Resource__Origin__EchoQuery_Immediate, m_localProxyServer.Server );
            m_echoProviderProxy_Delayed   = new EchoProviderProxy_Delayed  ( TestConstants.Resource__Origin__EchoQuery_Delayed  , m_localProxyServer.Server );

            m_localProxyServer.AddProvider( TestConstants.Resource__Origin__EchoQuery_Immediate, m_echoProviderProxy_Immediate );
            m_localProxyServer.AddProvider( TestConstants.Resource__Origin__EchoQuery_Delayed  , m_echoProviderProxy_Delayed   );

            //
            // Remote providers on remote origin server
            //
            m_originServer.AddProvider( TestConstants.Resource__Origin__EchoQuery_Immediate, new EchoProvider_Immediate( ) );
            m_originServer.AddProvider( TestConstants.Resource__Origin__EchoQuery_Delayed  , new EchoProvider_Delayed  ( ) );

            m_originServer.Start( ); 
            m_localProxyServer .Start( );

            m_client = new UdpTestClient( TestConstants.Client__LocalOriginEndPoint__8081 );

            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public virtual void CleanUp( )
        {
            Log.Comment( "*** Cleaning up after the tests" );
            Log.NewLine( );
            Log.NewLine( );

            m_client.Disconnect( );

            m_localProxyServer .Stop( );
            m_originServer.Stop( );
        }

        //--//

        protected void ClearCachesAndStatistics( )
        {
            m_localProxyServer.Server.EmptyCache( );

            m_echoProviderProxy_Immediate.ClearLocalCache( );
            m_echoProviderProxy_Delayed  .ClearLocalCache( );

            m_client          .Statistics.Clear( );
            m_originServer    .Statistics.Clear( );
            m_localProxyServer.Statistics.Clear( );
        }

        protected void ClearStatistics( )
        {
            m_client      .Statistics.Clear( );
            m_originServer.Statistics.Clear( );
            m_localProxyServer .Statistics.Clear( );
        }

        protected static void TEST_DELAY( )
        {
            System.Threading.Thread.Sleep( 1000 ); 
        }
    }
}
