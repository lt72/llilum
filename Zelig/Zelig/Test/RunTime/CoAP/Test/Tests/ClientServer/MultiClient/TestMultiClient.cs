//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;


    public class TestMultiClient : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            res |= OpenCloseTwice( );

            return res;
        }

        [TestMethod]
        public TestResult OpenCloseTwice( )
        {
            Log.Comment( "*** Open/Close a client twice in a raw and send one confirmable request for a known IMMEDIATE resource." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 2,
                ImmediateResposesReceived = 2,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived      = 2,
                ImmediateResposesSent = 2,
            };

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // Use same request
                //

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .Build( );

                //
                // 1st request
                //

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version      ( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type         ( response, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code         ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken    ( request, response );

                //
                // 2nd request, re-use previous builder and request
                //

                m_client.Disconnect( );
                m_client.Connect( null, resource );

                var request1 = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .Build( );

                var response1 = m_client.MakeRequest( request1 );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version      ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type         ( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code         ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request1, response1 );
                CoApTestAsserts.Assert_SameToken    ( request1, response1 );

                CoApTestAsserts.Assert_Statistics( m_client          .Statistics, desiredClientStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
