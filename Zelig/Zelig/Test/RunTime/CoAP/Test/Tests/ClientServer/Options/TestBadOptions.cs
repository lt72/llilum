//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using System.Net;
    using System.Threading;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;

    public class TestBadOptions : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            //
            // Upon reception, unrecognized options of class "critical" MUST be rejected and 
            // client must bail out immediately.
            //
            res |= ReceiveResponseWithCriticalUnsupportedOption_Confirmable( );

            //
            // Upon reception, unrecognized options of class "elective" MUST be silently ignored.
            //
            res |= SendRequestWithElectiveUnsupportedOption_Confirmable   ( );
            res |= SendRequestWithElectiveUnsupportedOption_NonConfirmable( );

            //
            // Unrecognized options of class "critical" that occur in a
            // Confirmable request MUST cause the return of a 4.02( Bad Option )
            // response. This response SHOULD include a diagnostic payload
            // describing the unrecognized option( s )( see Section 5.5.2 ).
            //
            res |= SendRequestWithCriticalUnsupportedOption_Confirmable( );

            //
            // Unrecognized options of class "critical" that occur in a Non-
            // confirmable message MUST cause the message to be rejected
            // (Section 4.3 ).
            //
            res |= SendRequestWithCriticalUnsupportedOption_NonConfirmable( );
            //res |= SendResponseWithCriticalUnsupportedOption_NonConfirmable( );

            //
            // Unrecognized options of class "critical" that occur in a
            // Confirmable response, or piggybacked in an Acknowledgement, MUST
            // cause the response to be rejected( Section 4.2).
            //
            //res |= SendResponseWithCriticalUnsupportedOption_Confirmable_Piggyback( );
            //res |= SendResponseWithCriticalUnsupportedOption_Confirmable_Delayed  ( );

            return res;
        }

        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 

        //
        // Critical/Unsupported options
        //


        [TestMethod]
        public TestResult ReceiveResponseWithCriticalUnsupportedOption_Confirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with a non-standard unsupported critical option, and verify response is correct." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent = 1,
                ResetsSent   = 1,
                Errors       = 1,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived      = 1,
                ImmediateResposesSent = 1,
                ResetsReceived        = 1,
            };

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                m_localProxyServer.MessagingMock.OnOutgoingMessageMock += MessageMockHandler_AnswerWithBadOption;

                m_localProxyServer.MessagingMock.AnswerWithBadOption = true;

                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .Build( );

                var response = m_client.MakeRequest( request );

                if(response != null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Statistics( m_client.Statistics     , desiredClientStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
            }
            finally
            {
                m_localProxyServer.MessagingMock.OnIncomingMessageMock -= MessageMockHandler_AnswerWithBadOption;

                m_localProxyServer.MessagingMock.AnswerWithBadOption = false;

                m_client.Disconnect( );
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }
        
        private bool MessageMockHandler_AnswerWithBadOption( object sender, ref CoAPMessageEventArgs args )
        {
            //
            // Inject a bogus critical option that the client cannot possibly handle
            // 
            if(m_localProxyServer.MessagingMock.AnswerWithBadOption)
            {
                Logger.Instance.LogWarning( $"*** Request received '{args.MessageContext.Message}', injecting bad option..." );

                //
                // !!! MUST match test !!!
                // 
                var resource = TestConstants.Resource__EchoQuery_Immediate;

                var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Critical_UnSafe_NoCacheKey;

                CoApTestAsserts.Assert( MessageOption.IsSupportedOption( unrecognizedOptionNumber ) == false );
                CoApTestAsserts.Assert( MessageOption.IsCriticalOption( (byte)unrecognizedOptionNumber ) == true );
                CoApTestAsserts.Assert( MessageOption.IsSafeOption( (byte)unrecognizedOptionNumber ) == false );
                CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true );

                var intercepted = args.MessageContext.Message;

                CoAPMessage oldMsg = intercepted as CoAPMessage;

                if(oldMsg == null)
                {
                    oldMsg = CoAPMessage.FromBufferWithContext( intercepted.Buffer, args.MessageContext );

                    using(var parser = MessageParser.CheckOutParser( ))
                    {

                        bool fCorrect = CoAPMessage.ParseFromBuffer( intercepted.Buffer, parser, ref oldMsg );
                    }
                }

                var msg = MessageBuilder.Create( null, resource )
                    .WithHeader   ( oldMsg.Header  )
                    .WithMessageId( oldMsg.MessageId )
                    .WithToken    ( oldMsg.Token   )
                    .WithOptions  ( oldMsg.Options )
                    .WithPayload  ( oldMsg.Payload )
                    .WithOption   ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
                    .Build( );

                var buffer = msg.Buffer;

                args.MessageContext.Message = msg;

                Logger.Instance.LogWarning( $"*** New request      '{args.MessageContext.Message}'" );
            }

            return true;
        }

        //--//

        [TestMethod]
        public TestResult SendRequestWithElectiveUnsupportedOption_Confirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with a non-standard elective option, and verify response is correct." );
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

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Elective_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsSafeOption      ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
                .Build( );

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
        public TestResult SendRequestWithElectiveUnsupportedOption_NonConfirmable( )
        {
            Log.Comment( "*** Send one non-confirmable request for a known IMMEDIATE resource with a non-standard elective option, and verify response is correct." );
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

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Elective_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsSafeOption      ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
                .Build( );

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
        public TestResult SendRequestWithCriticalUnsupportedOption_Confirmable( )
        {
            Log.Comment( "*** Send one confirmable request for a known IMMEDIATE resource with a non-standard critical option, and verify response is correct." );
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

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;
            var payload  = new byte[] { 0xAA, 0xBB }; 

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Critical_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsSafeOption      ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, payload ) )
                .Build( );

                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.BadOption );
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );
                CoApTestAsserts.Assert_Payload( payload, response.Payload );

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
        public TestResult SendRequestWithCriticalUnsupportedOption_NonConfirmable( )
        {
            Log.Comment( "*** Send one non-confirmable request for a known IMMEDIATE resource with a non-standard critical option, and verify response is correct." );
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

            ClearCachesAndStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Critical_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsSafeOption      ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
                .Build( );

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
