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


    public class TestContentFormat : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= SendRequestWithTextContentFormat_Confirmable   ( );
            res |= SendRequestWithJsonContentFormat_Confirmable   ( );
            res |= SendRequestWithJsonContentFormat_NonConfirmable( );

            return res;
        }

        private void ClearStatistics()
        {
            m_client.Statistics.Clear( );
            m_server.Statistics.Clear( );
        }

        [TestMethod]
        public TestResult SendRequestWithTextContentFormat_Confirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with an Accept option for text/plain." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                ImmediateResposesReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived      = 1,
                ImmediateResposesSent = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_UInt.New( MessageOption.OptionNumber.Accept, MessageOption.ContentFormat.Text_Plain__UTF8 ) )
                .BuildAndReset( );

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );

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

        [TestMethod]
        public TestResult SendRequestWithJsonContentFormat_Confirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with an Accept option for application/json." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                ImmediateResposesReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                Errors                = 1,
                ImmediateResposesSent = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_UInt.New( MessageOption.OptionNumber.Accept, MessageOption.ContentFormat.Application__Json ) )
                .BuildAndReset( );

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.NotAcceptable );
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );

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
        

        [TestMethod]
        public TestResult SendRequestWithJsonContentFormat_NonConfirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with an Accept option for application/json." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent   = 1,
                ResetsReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                Errors     = 1,
                ResetsSent = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_UInt.New( MessageOption.OptionNumber.Accept, MessageOption.ContentFormat.Application__Json ) )
                .BuildAndReset( );

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Reset );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Request, CoAPMessage.Detail_Request.Empty );
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );

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
