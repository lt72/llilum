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


    public class TestUnsupportedMethods : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= NotGET( );

            return res;
        }

        [TestMethod]
        public TestResult NotGET( )
        {
            Log.Comment( "*** Send one request for an unsupported method for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                ImmediateResposesReceived = 3,
            };
            var desiredServerStats = new Statistics()
            {
                Errors                = 3,
                RequestsReceived      = 3,
                ImmediateResposesSent = 3,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // POST
                //
                var postRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.POST )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                    .BuildAndReset( );
                
                var postResponse = m_client.MakeRequest( postRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( postResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( postResponse, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( postResponse, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.MethodNotAllowed );
                CoApTestAsserts.Assert_SameMessageId( postRequest, postResponse );
                CoApTestAsserts.Assert_SameToken( postRequest, postResponse );

                //
                // PUT
                //
                var putRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.PUT )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                    .BuildAndReset( );
                
                var putResponse = m_client.MakeRequest( putRequest );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( putResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( putResponse, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( putResponse, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.MethodNotAllowed );
                CoApTestAsserts.Assert_SameMessageId( putRequest, putResponse );
                CoApTestAsserts.Assert_SameToken( putRequest, putResponse );

                //
                // DELETE
                //
                var deleteRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.DELETE )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                    .BuildAndReset( );
                
                var deleteResponse = m_client.MakeRequest( deleteRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( deleteResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( deleteResponse, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( deleteResponse, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.MethodNotAllowed );
                CoApTestAsserts.Assert_SameMessageId( deleteRequest, deleteResponse );
                CoApTestAsserts.Assert_SameToken( deleteRequest, deleteResponse );

                //--// 

                CoApTestAsserts.Assert_Statistics( m_client.Statistics, desiredClientStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_server.Statistics, desiredServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
