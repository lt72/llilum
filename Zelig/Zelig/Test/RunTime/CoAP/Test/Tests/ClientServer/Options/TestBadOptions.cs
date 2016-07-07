//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using System.Net;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;


    public class TestBadOptions : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            //
            // Upon reception, unrecognized options of class "elective" MUST be silently ignored.
            //
            res |= SendRequestWithElectiveUnsupportedOption_Confirmable( );
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

            res |= SendRequestWithProxyUri_SameHost                                 ( ); // SEE: https://github.com/lt72/CoAP-pr/issues/36
            res |= SendRequestWithProxyUri_SameHost_DestinationNotNull              ( ); // SEE: https://github.com/lt72/CoAP-pr/issues/36
            //res |= SendRequestWithProxyUri_DifferentHost_ReverseProxy_Confimable    ( );
            //res |= SendRequestWithProxyUri_DifferentHost_ForwardProxy_Confimable    ( );
            //res |= SendRequestWithProxyUri_DifferentHost_ForwardProxy_NonConfirmable( );

            return res;
        }

        private void ClearStatistics()
        {
            m_client.Statistics.Clear( );
            m_server.Statistics.Clear( );
        }

        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 

        //
        // Critical/Unsupported options
        //
        
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

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Elective_UnSafe_NoCacheKey;
            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsUnsafeOption    ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
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

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Elective_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsUnsafeOption    ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
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

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;
            var payload  = new byte[] { 0xAA, 0xBB }; 

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Critical_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption(        unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsUnsafeOption    ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, payload ) )
                .BuildAndReset( );

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
                CoApTestAsserts.Assert_Payload( payload, response );

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

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            var unrecognizedOptionNumber = TestConstants.UnsupportedOption__Critical_UnSafe_NoCacheKey;

            CoApTestAsserts.Assert( MessageOption.IsSupportedOption (       unrecognizedOptionNumber ) == false );
            CoApTestAsserts.Assert( MessageOption.IsCriticalOption  ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsUnsafeOption    ( (byte)unrecognizedOptionNumber ) == true  );
            CoApTestAsserts.Assert( MessageOption.IsNoCacheKeyOption( (byte)unrecognizedOptionNumber ) == true  );

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, resource.Path ) )
                .WithOption     ( MessageOption_Opaque.New( unrecognizedOptionNumber, new byte[] { 0xAA, 0xBB} ) )
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

        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 
        //~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~o~~// 

        //
        // Proxy
        //

        [TestMethod]
        public TestResult SendRequestWithProxyUri_SameHost( )
        {
            Log.Comment( "*** Send two non-confirmable requests for a known IMMEDIATE resource with Fw proxy option set to same host connection is attempted to." );
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

            var proxyUri = new ServerCoAPUri( TestConstants.EndPoint__8080, "res" );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path , resource.Path ) )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
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
        public TestResult SendRequestWithProxyUri_SameHost_DestinationNotNull( )
        {
            Log.Comment( "*** Send two non-confirmable requests for a known IMMEDIATE resource with Fw proxy option set to same host connection is attempted to." );
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

            var proxyUri = new ServerCoAPUri( TestConstants.EndPoint__8080, "res" );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( proxyUri.EndPoints[0], resource );

                var request = messageBuilder
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                .WithTokenLength( Defaults.TokenLength )
                .WithRequestCode( CoAPMessage.Detail_Request.GET )
                .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
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
        public TestResult SendRequestWithProxyUri_DifferentHost_ReverseProxy_Confimable( )
        {
            Log.Comment( "*** Send one non-confirmable request for a known IMMEDIATE resource with a non-standard critical option, and verify response is correct." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 2,
                ImmediateResposesReceived = 2,
            };
            var desiredServerStats = new Statistics()
            {
                Errors                = 2,
                ImmediateResposesSent = 2,
            };

            ClearStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            var proxyUri = new ServerCoAPUri( TestConstants.EndPoint__8080, "res" );

            var resource = TestConstants.Resource__EchoQuery_Immediate_OtherPort;

            try
            {
                //
                // Use explicit intermediary
                //
                {
                    var messageBuilder = m_client.Connect( proxyUri.EndPoints[0], resource );

                    var request = messageBuilder
                        .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                        .WithType       ( CoAPMessage.MessageType.Confirmable )
                        .WithTokenLength( Defaults.TokenLength )
                        .WithRequestCode( CoAPMessage.Detail_Request.GET )
                        .BuildAndReset( );

                    var response = m_client.MakeRequest( request );

                    // CON messages must be acknowledged...
                    if(response == null)
                    {
                        Log.Comment( "*** COMPLETED: FAIL" );
                        return TestResult.Fail;
                    }

                    CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                    CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Acknowledgement );
                    CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.ServerError, CoAPMessage.Detail_ServerError.ProxyingNotSupported );
                    CoApTestAsserts.Assert_SameMessageId( request, response );
                    CoApTestAsserts.Assert_SameToken( request, response );
                }

                //
                // Use Uri-Host and Uri-Port
                //
                {
                    m_client.Disconnect( );
                    
                    var messageBuilder1 = m_client.Connect( proxyUri.EndPoints[0], null );

                    var request1 = messageBuilder1
                        .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                        .WithType       ( CoAPMessage.MessageType.Confirmable )
                        .WithTokenLength( Defaults.TokenLength )
                        .WithRequestCode( CoAPMessage.Detail_Request.GET )
                        .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Host,       resource.EndPoints[0].Address.ToString( ) ) ) // use same resource 
                        .WithOption     ( MessageOption_UInt  .New( MessageOption.OptionNumber.Uri_Port, (uint)resource.EndPoints[0].Port                ) )
                        .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path,       resource.Path                            ) )
                        .BuildAndReset( );

                    var response1 = m_client.MakeRequest( request1 );

                    // CON messages must be acknowledged...
                    if(response1 == null)
                    {
                        Log.Comment( "*** COMPLETED: FAIL" );
                        return TestResult.Fail;
                    }

                    CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                    CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Acknowledgement );
                    CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.ServerError, CoAPMessage.Detail_ServerError.ProxyingNotSupported );
                    CoApTestAsserts.Assert_SameMessageId( request1, response1 );
                    CoApTestAsserts.Assert_SameToken( request1, response1 );
                }

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
        public TestResult SendRequestWithProxyUri_DifferentHost_ForwardProxy_Confimable( )
        {
            Log.Comment( "*** Send one non-confirmable request for a known IMMEDIATE resource with a non-standard critical option, and verify response is correct." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent            = 1,
                RequestsRetransmissions = 3,
            };
            var desiredServerStats = new Statistics()
            {
            };

            ClearStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            IPEndPoint endPoint = new IPEndPoint( Utils.AddressFromHostName( "google.com" ), 8089 );

            var proxyUri = new ServerCoAPUri( endPoint, "res" );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( proxyUri.EndPoints[0], resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
                    .BuildAndReset( );

                var response = m_client.MakeRequest( request );

                // CON messages must be acknoledged, but only if there is actually a server to respond ...
                if(response != null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

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
        public TestResult SendRequestWithProxyUri_DifferentHost_ForwardProxy_NonConfirmable( )
        {
            Log.Comment( "*** Send one non-confirmable request for a known IMMEDIATE resource with a non-standard critical option, and verify response is correct." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                RequestsSent            = 1,
                RequestsRetransmissions = 3,
            };
            var desiredServerStats = new Statistics()
            {
            };

            ClearStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            IPEndPoint endPoint = new IPEndPoint( Utils.AddressFromHostName( "google.com" ), 8089 );

            var proxyUri = new ServerCoAPUri( endPoint, "res" );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( proxyUri.EndPoints[0], resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
                    .BuildAndReset( );

                var response = m_client.MakeRequest( request );

                // NON messages must be ignored...
                if(response != null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

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
