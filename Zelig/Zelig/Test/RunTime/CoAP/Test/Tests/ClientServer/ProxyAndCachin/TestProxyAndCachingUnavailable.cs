//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using System.Net;
    using Microsoft.Zelig.Test;
    using CoAP.Stack;
    using Test.ClientServerUtils;
    using CoAP.Common.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack.Abstractions;
    using System;

    public class TestProxyAndCachingUnavailable : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            //
            // Unreachable proxy
            //         

            res |= VanillaProxy_Immediate__Unreachable( 1 );
            res |= VanillaProxy_Immediate__Unreachable( 2 );

            return res;
        }
        
        [TestMethod]
        public TestResult VanillaProxy_Immediate__Unreachable( int iterations )
        {
            Log.Comment( "*** PROXY: Send one request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats do not reflect the cache warm-up because this provider is always cold.
            // 
            var desiredClientStats = new Statistics()
            {
                AcksReceived              = 3,
                AcksSent                  = 1,       // this is for the confirmable delayed response
                DelayedResponsesReceived  = 1,
                RequestsRetransmissions   = 2,
                RequestsSent              = 1,
            } * iterations;
            var desiredLocalServerStats = new Statistics()
            {
                AcksSent              = 1,
                AcksReceived          = 1,           // this is for the client acking the confirmable delayed response
                CacheMisses           = 2,
                DelayedResponsesSent  = 1,
                ImmediateResposesSent = 2,
                RequestsReceived      = 1,
            } * iterations;
            var desiredRemoteServerStats = new Statistics()
            {
            } * iterations;


            ClearCachesAndStatistics( );

            //--//

            var resource = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                m_originServer.MessagingMock.OnIncomingMessageMock += MessageMockHandler_DiscardMessage;

                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .Build( );

                int count = iterations;
                while(count-- > 0)
                {

                    var response = m_client.MakeRequest( request );

                    if(response == null)
                    {
                        Log.Comment( "*** COMPLETED: FAIL" );
                        return TestResult.Fail;
                    }

                    CoApTestAsserts.Assert_Version         ( response, CoAPMessage.ProtocolVersion.Version_1 );
                    CoApTestAsserts.Assert_Type            ( response, CoAPMessage.MessageType.Confirmable );
                    CoApTestAsserts.Assert_Code            ( response, CoAPMessage.Class.ServerError, CoAPMessage.Detail_ServerError.GatewayTimeout );
                    CoApTestAsserts.Assert_NotSameMessageId( request, response );
                    CoApTestAsserts.Assert_SameToken       ( request, response );
                }

                CoApTestAsserts.Assert_Statistics( m_client          .Statistics, desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer    .Statistics, desiredRemoteServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
            }
            finally
            {
                m_originServer.MessagingMock.OnIncomingMessageMock -= MessageMockHandler_DiscardMessage;

                m_client.Disconnect( );
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }

        private bool MessageMockHandler_DiscardMessage( object sender, ref CoAPMessageEventArgs args )
        {
            return false;
        }  
    }
}
