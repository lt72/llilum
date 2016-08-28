//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using CoAP.Stack;
    using Test.ClientServerUtils;
    using CoAP.Common.Diagnostics;
    using CoAP.Common;


    public class TestDelayedResponse : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= VanillaRequestResponse__Delayed      ( );
            res |= VanillaRequestResponse__Delayed_Twice( );

            return res;
        }

        [TestMethod]
        public TestResult VanillaRequestResponse__Delayed( )
        {
            Log.Comment( "*** Send one request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent                = 1,
                AcksReceived                = 1,
                DelayedResponsesReceived   = 1,
                AcksSent                    = 1,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived    = 1,
                AcksReceived        = 1,
                DelayedResponsesSent = 1,
                AcksSent            = 1,
            };

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .Build( );

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );

                CoApTestAsserts.Assert_Statistics( m_client.Statistics, desiredClientStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
            }
            finally
            {
                m_client.Disconnect( );
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult VanillaRequestResponse__Delayed_Twice( )
        {
            Log.Comment( "*** Send two requests for a known DELAYED resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 2,
                AcksReceived              = 2,
                DelayedResponsesReceived = 2,
                AcksSent                  = 2,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived     = 2,
                AcksSent             = 2,
                DelayedResponsesSent = 2,
                AcksReceived         = 2,
            };

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .Build( );

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );

                var response2 = m_client.MakeRequest( request );

                if(response2 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response2, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response2, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response2, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response2 );
                CoApTestAsserts.Assert_SameToken( request, response2 );

                CoApTestAsserts.Assert_Statistics( m_client.Statistics, desiredClientStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
            }
            finally
            {
                m_client.Disconnect( );
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }
    }
}
