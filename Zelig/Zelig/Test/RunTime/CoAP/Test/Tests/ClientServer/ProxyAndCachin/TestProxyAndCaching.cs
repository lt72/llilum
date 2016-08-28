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

    public class TestProxyAndCaching : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            //
            // Simple proxying 
            // 
            //res |= VanillaProxy_Immediate_Repeated_Valid_Etag_Expire_And_Refresh( );

            res |= VanillaProxy_Immediate( 1 );
            res |= VanillaProxy_Immediate( 1 );
            res |= VanillaProxy_Immediate( 3 );
            res |= VanillaProxy_Immediate( 10 );

            res |= VanillaProxy_Delayed( 1 );
            res |= VanillaProxy_Delayed( 1 );
            res |= VanillaProxy_Delayed( 3 );
            res |= VanillaProxy_Delayed( 10 );


            res |= VanillaProxy_Immediate_Repeated_Valid( );
            res |= VanillaProxy_Immediate_Repeated_Valid_Etag( );
            
            //
            // Proxy URI
            //

            res |= SendRequestWithProxyUri_SameHost                                 ( ); 
            res |= SendRequestWithProxyUri_SameHost_IntermediaryNotNull             ( );
            res |= SendRequestWithProxyUri_DifferentHost_ReverseProxy_Confimable( );
            res |= SendRequestWithProxyUri_DifferentHost_ForwardProxy_Confimable( );
            res |= SendRequestWithProxyUri_DifferentHost_ForwardProxy_NonConfirmable( );

            return res;
        }

        [TestMethod]
        public TestResult VanillaProxy_Immediate( int iterations )
        {
            Log.Comment( "*** PROXY: Send one request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats need to reflect the cache warm-up. The first request will yield a cache miss and a delayed answer, but all successive request MUST 
            // yield a cache hit and an immediate answer.
            // 
            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1 *  iterations,
                AcksReceived              = 1,
                DelayedResponsesReceived  = 1,
                ImmediateResposesReceived = 1 * (iterations  - 1),
                AcksSent                  = 1,                      // this is for the confirmable delayed response
            };
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived      = 1 *  iterations,
                AcksReceived          = 1,                          // this is for the client acking the confirmable delayed response
                DelayedResponsesSent  = 1,
                ImmediateResposesSent = 1 * (iterations  - 1),
                AcksSent              = 1,
                CacheHits             = 1 * (iterations  - 1),
                CacheMisses           = 2,
            };
            var desiredRemoteServerStats = new Statistics()
            {
                RequestsReceived      = 1,
                ImmediateResposesSent = 1,
            };
            
            ClearCachesAndStatistics( );

            //--//

            var resource = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                int count = iterations;
                while(count-- > 0)
                {
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
                    if(count == iterations - 1)
                    {
                        CoApTestAsserts.Assert_Type            ( response, CoAPMessage.MessageType.Confirmable );
                        CoApTestAsserts.Assert_Code            ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                        CoApTestAsserts.Assert_NotSameMessageId( request, response );
                    }
                    else
                    {
                        CoApTestAsserts.Assert_Type         ( response, CoAPMessage.MessageType.Acknowledgement );
                        CoApTestAsserts.Assert_Code         ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Valid );
                        CoApTestAsserts.Assert_SameMessageId( request, response );
                    }
                    CoApTestAsserts.Assert_SameToken( request, response );
                }

                CoApTestAsserts.Assert_Statistics( m_client          .Statistics, desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer    .Statistics, desiredRemoteServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult VanillaProxy_Delayed( int iterations )
        {
            Log.Comment( "*** PROXY: Send one request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats do not reflect the cache warm-up because this provider is always cold.
            // 
            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                AcksReceived              = 1,
                DelayedResponsesReceived  = 1,
                AcksSent                  = 1,       // this is for the confirmable delayed response
            } * iterations;
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived      = 1,
                AcksReceived          = 1,           // this is for the client acking the confirmable delayed response
                DelayedResponsesSent  = 1,
                AcksSent              = 1,
                CacheMisses           = 2,
            } * iterations;
            var desiredRemoteServerStats = new Statistics()
            {
                AcksReceived          = 1,
                AcksSent              = 1,
                RequestsReceived      = 1,
                DelayedResponsesSent  = 1,
            } * iterations;
            
            ClearCachesAndStatistics( );

            //--//

            var resource = TestConstants.Resource__ProxyMoniker__EchoQuery_Delayed;

            try
            {
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

                    CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                    CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Confirmable );
                    CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                    CoApTestAsserts.Assert_NotSameMessageId( request, response );                    
                    CoApTestAsserts.Assert_SameToken( request, response );
                }

                CoApTestAsserts.Assert_Statistics( m_client          .Statistics, desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer    .Statistics, desiredRemoteServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
            }
            finally
            {
                m_client.Disconnect( );
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }

        //--//

        [TestMethod]
        public TestResult VanillaProxy_Immediate__Unreachable( int iterations )
        {
            Log.Comment( "*** PROXY: Send one request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats do not reflect the cache warm-up because this provider is always cold.
            // 
            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 1,
                AcksReceived              = 4,
                DelayedResponsesReceived  = 1,
                RequestsRetransmissions   = 3,
                AcksSent                  = 1,       // this is for the confirmable delayed response
            } * iterations;
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived      = 4,
                AcksReceived          = 1,           // this is for the client acking the confirmable delayed response
                DelayedResponsesSent  = 4,
                AcksSent              = 4,
                CacheMisses           = 4,
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

                    CoApTestAsserts.Assert_Version( response, CoAPMessage.ProtocolVersion.Version_1 );
                    CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Confirmable );
                    CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.ServerError, CoAPMessage.Detail_ServerError.GatewayTimeout );
                    CoApTestAsserts.Assert_NotSameMessageId( request, response );
                    CoApTestAsserts.Assert_SameToken( request, response );
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

        //--//

        [TestMethod]
        public TestResult VanillaProxy_Immediate_Repeated_Valid( )
        {
            Log.Comment( "*** PROXY: Send one request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats need to reflect the cache warm-up. The first request will yield a cache miss and a delayed answer, but all successive request MUST 
            // yield a cache hit and an immediate answer.
            // 
            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 2,
                AcksReceived              = 1,
                DelayedResponsesReceived  = 1,
                ImmediateResposesReceived = 1,
                AcksSent                  = 1,                      // this is for the confirmable delayed response
            };
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived      = 2,
                AcksReceived          = 1,                          // this is for the client acking the confirmable delayed response
                DelayedResponsesSent  = 1,
                ImmediateResposesSent = 1,
                AcksSent              = 1,
                CacheHits             = 1,
                CacheMisses           = 2,
            };
            var desiredRemoteServerStats = new Statistics()
            {
                RequestsReceived      = 1,
                ImmediateResposesSent = 1,
            };

            ClearCachesAndStatistics( );

            //--//

            var resource = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                //
                // First request to warm up caches
                // 
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
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( TestConstants.Resource__EchoQuery_Immediate.Path ), response1.Payload );

                var response2 = m_client.MakeRequest( request );

                if(response2 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response2, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response2, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response2, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Valid );
                CoApTestAsserts.Assert_SameMessageId( request, response2 );
                CoApTestAsserts.Assert_SameToken( request, response2 );
                CoApTestAsserts.Assert_Payload( MessagePayload.EmptyPayload, response2.Payload );

                //--//

                CoApTestAsserts.Assert_Statistics( m_client.Statistics      , desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics , desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer.Statistics, desiredRemoteServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult VanillaProxy_Immediate_Repeated_Valid_Etag( )
        {
            Log.Comment( "*** PROXY: Send one request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            //
            // Stats need to reflect the cache warm-up. The first request will yield a cache miss and a delayed answer, but all successive request MUST 
            // yield a cache hit and an immediate answer.
            // 
            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                AcksReceived              = 2,
                DelayedResponsesReceived  = 2,
                ImmediateResposesReceived = 1,
                AcksSent                  = 2,                      // this is for the confirmable delayed response
            };
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived      = 3,
                AcksReceived          = 2,                          // this is for the client acking the confirmable delayed response
                DelayedResponsesSent  = 2,
                ImmediateResposesSent = 1,
                AcksSent              = 2,
                CacheHits             = 1,
                CacheMisses           = 4,
            };
            var desiredRemoteServerStats = new Statistics()
            {
                RequestsReceived      = 2,
                ImmediateResposesSent = 2,
            };

            ClearCachesAndStatistics( );

            //--//

            var resource = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                //
                // 1st request to warm up caches
                // 
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

                CoApTestAsserts.Assert_Version         ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken       ( request, response1 );
                CoApTestAsserts.Assert_Payload         ( Defaults.Encoding.GetBytes( TestConstants.Resource__EchoQuery_Immediate.Path ), response1.Payload );

                var ETag = response1.Options.ETag;

                //
                // 2nd request same resource with retrieved ETag, should use the fresh cache value 
                //
                var request2 = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( ETag )
                    .Build( );

                var response2 = m_client.MakeRequest( request2 );

                if(response2 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response2, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response2, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response2, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Valid );
                CoApTestAsserts.Assert_SameMessageId( request2, response2 );
                CoApTestAsserts.Assert_SameToken( request2, response2 );
                CoApTestAsserts.Assert_Payload( MessagePayload.EmptyPayload, response2.Payload );

                //
                // Request same resource with different ETag, should cause retrieving a new value
                //
                //var ETag3 = new byte[] { (byte)(((byte[ ])ETag.Value)[ 0 ] + 1), ((byte[ ])ETag.Value)[ 1 ], ((byte[ ])ETag.Value)[ 2 ], ((byte[])ETag.Value)[ 3 ] }; 

                ((byte[])ETag.RawBytes)[ 0 ] = (byte)(((byte[ ])ETag.RawBytes)[ 0 ] + 1);

                var request3 = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    //.WithOption     ( MessageOption_Opaque.New(MessageOption.OptionNumber.ETag, ETag3 ) )
                    .WithOption     ( ETag )
                    .Build( );

                var response3 = m_client.MakeRequest( request3 );

                if(response3 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response3, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response3, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response3, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request3, response3 );
                CoApTestAsserts.Assert_SameToken( request3, response3 );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( TestConstants.Resource__EchoQuery_Immediate.Path ), response3.Payload );

                //--//

                CoApTestAsserts.Assert_Statistics( m_client.Statistics      , desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics , desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer.Statistics, desiredRemoteServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        // Proxy URI
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

            ClearCachesAndStatistics( );

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Same proxy and resource 
            var proxyUri = TestConstants.Resource__EchoQuery_Immediate;
            var resource = proxyUri;
            //
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
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
        public TestResult SendRequestWithProxyUri_SameHost_IntermediaryNotNull( )
        {
            Log.Comment( "*** Send two non-confirmable requests for a known IMMEDIATE resource with Fw proxy option set to same host and intermediary the connection is attempted to." );
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

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Same proxy and resource 
            var proxyUri = TestConstants.Resource__EchoQuery_Immediate;
            var resource = proxyUri;
            //
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            try
            {
                var messageBuilder = m_client.Connect( TestConstants.Server__LocalOriginEndPoint__8080, resource );

                var request = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.NonConfirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Proxy_Uri, proxyUri.ToString( ) ) )
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

            ClearCachesAndStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            var proxyUri = new CoAPServerUri( TestConstants.Server__LocalOriginEndPoint__8080, "res" );

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
                        .Build( );

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
                        .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Host, resource.EndPoints[0].Address.ToString( ) ) ) // use same resource 
                        .WithOption     ( MessageOption_Int  .New( MessageOption.OptionNumber.Uri_Port, resource.EndPoints[0].Port                ) )
                        .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, "res"                                     ) )
                        .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, "echo-query-immediate"                    ) )
                        .Build( );

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

            ClearCachesAndStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            IPEndPoint endPoint = new IPEndPoint( Utils.AddressFromHostName( "google.com" ), 8089 );

            var proxyUri = new CoAPServerUri( endPoint, "res" );

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
                    .Build( );

                var response = m_client.MakeRequest( request );

                // CON messages must be acknoledged, but only if there is actually a server to respond ...
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

            ClearCachesAndStatistics( );

            //
            // Choose a host name we can connect to, but that does not match the test server host name ('localhost')
            // 
            IPEndPoint endPoint = new IPEndPoint( Utils.AddressFromHostName( "google.com" ), 8089 );

            var proxyUri = new CoAPServerUri( endPoint, "res" );

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
                    .Build( );

                var response = m_client.MakeRequest( request );

                // NON messages must be ignored...
                if(response != null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Statistics( m_client.Statistics, desiredClientStats     , TransmissionParameters.default_EXCHANGE_LIFETIME );
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
