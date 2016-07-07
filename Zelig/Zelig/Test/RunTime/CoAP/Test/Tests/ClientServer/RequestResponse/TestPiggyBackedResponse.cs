//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using System.Threading;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;


    public class TestPiggyBackedResponse : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= EmptyReset_CoAPPing                        ( );
            res |= EmptyReset_CoAPPing_HostAndPort            ( );
            res |= VanillaRequestResponse_Confirmable         ( );
            res |= VanillaRequestResponse__Confirmable__Twice ( );
            res |= DropResponseOnce__Confirmable              ( );
            res |= DropRequestOnce__Confirmable               ( );
            res |= VanillaRequestResponse_Confirmable_NotFound( ); 

            return res;
        }

        //--//

        private bool MessageMockHandler_DiscardFirstMessage( object sender, ref CoAPMessageEventArgs args )
        {
            //
            // Drop first message received
            // 
            int dropped = m_client.MessagingMock.DropResponseCount;

            if(Interlocked.Decrement( ref dropped ) >= 0)
            {
                Logger.Instance.LogWarning( "*** Response received, simulating drop..." );

                m_client.MessagingMock.DropResponseCount = dropped;

                return false;
            }

            return true;
        }

        //--//

        [TestMethod]
        public TestResult EmptyReset_CoAPPing( )
        {
            Log.Comment( "*** Send empty message (CoAP Ping) and verify that a reset is received" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent   = 1,
                ResetsReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                ResetsSent       = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder.CreateEmptyRequest().BuildAndReset( );

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

        [TestMethod]
        public TestResult EmptyReset_CoAPPing_HostAndPort( )
        {
            Log.Comment( "*** Send empty message (CoAP Ping) and verify that a reset is received" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent   = 1,
                ResetsReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                ResetsSent       = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder.CreateEmptyRequest()
                    .WithOption( MessageOption_String.New( MessageOption.OptionNumber.Uri_Host, "localhost" ))
                    .WithOption( MessageOption_UInt  .New( MessageOption.OptionNumber.Uri_Port, 8080        ))
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

        [TestMethod]
        public TestResult VanillaRequestResponse_Confirmable( )
        {
            Log.Comment( "*** Send one request for a known IMMEDIATE resource and verify that resource and answer code is as expected" );

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

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
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
        public TestResult VanillaRequestResponse_NonConfirmable( )
        {
            Log.Comment( "*** Send one request for a known IMMEDIATE resource and verify that resource and answer code is as expected" );

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

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .BuildAndReset( );


                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, new CoAPMessage.MessageType[ ] { CoAPMessage.MessageType.Confirmable, CoAPMessage.MessageType.NonConfirmable } );
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
        public TestResult VanillaRequestResponse__Confirmable__Twice( )
        {
            Log.Comment( "*** Send two requests for a known IMMEDIATE resource and verify that resource and answer code is as expected" );

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

            ClearStatistics( );

            var resource = TestConstants.Resource__PingForAck_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .BuildAndReset( );

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );


                var response2 = m_client.MakeRequest( request );

                CoApTestAsserts.Assert_Version( response2, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response2, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response2, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response2 );
                CoApTestAsserts.Assert_SameToken( request, response2 );

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
        public TestResult DropResponseOnce__Confirmable( )
        {
            Log.Comment( "+++ Send request message for a known IMMEDIATE resource and verify that a response is received even though first response is lost." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                ResetsReceived            = 1,
                RequestsRetransmissions   = 1,
            };
            var desiredServerStats = new Statistics()
            {
                ResetsSent              = 2,
            };

            ClearStatistics( );

            try
            {
                //
                // Setup mock, undo at the end of test
                // 
                m_client.MessagingMock.OnMessageMock += MessageMockHandler_DiscardFirstMessage;

                m_client.MessagingMock.DropResponseCount = 1;

                var resource = TestConstants.Resource__EchoQuery_Immediate;

                //
                // Start test...
                // 
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder.CreateEmptyRequest().BuildAndReset( );

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
                m_client.MessagingMock.OnMessageMock -= MessageMockHandler_DiscardFirstMessage;

                m_client.MessagingMock.DropResponseCount = 0;


                m_client.Disconnect( ); 
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult DropRequestOnce__Confirmable( )
        {
            Log.Comment( "+++ Send request message for a known IMMEDIATE resource and verify that a response is received even though first request is lost." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent            = 1,
                ResetsReceived          = 1,
                RequestsRetransmissions = 1,
            };
            var desiredServerStats = new Statistics()
            {
                ResetsSent = 1,
            };

            ClearStatistics( );

            try
            {
                //
                // Setup mock, undo at the end of test
                // 
                m_client.MessagingMock.DropRequestCount = 1;

                //
                // Start test...
                // 

                var resource = TestConstants.Resource__EchoQuery_Immediate;

                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder.CreateEmptyRequest().BuildAndReset( );

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
                m_client.MessagingMock.DropRequestCount = 0;

                m_client.Disconnect( ); 
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult VanillaRequestResponse_Confirmable_NotFound( )
        {
            Log.Comment( "*** Send one request for a known non-existing IMMEDIATE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                ImmediateResposesReceived = 1,
            };
            var desiredServerStats = new Statistics()
            {
                Errors                = 1,
                RequestsReceived      = 1,
                ImmediateResposesSent = 1,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__NotFound_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .BuildAndReset( );


                var response = m_client.MakeRequest( request );

                if(response == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.RequestError, CoAPMessage.Detail_RequestError.NotFound );
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
