//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using CoAP.Common.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack;
    using System.Threading;

    public class TestProxyAndCachingNonIdempotentMethods : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            res |= POST__ProxyImmediate  ( );
            res |= POST__ProxyDelayed    ( );
            res |= PUT__ProxyImmediate   ( );
            res |= PUT__ProxyDelayed     ( );
            res |= DELETE__ProxyImmediate( );
            res |= DELETE__proxyDelayed  ( );

            return res;
        }

        [TestMethod]
        public TestResult POST__ProxyImmediate( )
        {
            Log.Comment( "*** Send one POST request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                AcksReceived              = 2,
                AcksSent                  = 2,
                DelayedResponsesReceived  = 2, 
                ImmediateResposesReceived = 1,
            };
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived            = 3,
                ImmediateResposesSent       = 1,
                DelayedResponsesSent        = 2,
                AcksSent                    = 2,
                AcksReceived                = 2,
                CacheHits                   = 1,
                CacheMisses                 = 3,
            };
            var desiredOriginServerStats = new Statistics()
            {
                RequestsReceived            = 2,
                ImmediateResposesSent       = 2,
            };

            ClearCachesAndStatistics( );

            var originResource = TestConstants.Resource__EchoQuery_Immediate;
            var proxyResource  = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, proxyResource );

                //
                // Fetch default response (query)
                //
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

                CoApTestAsserts.Assert_Version         ( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request , response );
                CoApTestAsserts.Assert_SameToken       ( request , response ); 
                CoApTestAsserts.Assert_Payload         ( Defaults.Encoding.GetBytes( originResource.Path ), response.Payload );

                //
                // POST a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new CoAPServerUri( TestConstants.Server__LocalOriginEndPoint__8080, proxyResource.Path + "?echo=" + echoString );
                
                var postRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.POST )
                    .WithOptions    ( newUri.Options )
                    .WithPayload    ( MessagePayload_String.New( echoString ) )
                    .Build( );
                
                var postResponse = m_client.MakeRequest( postRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version         ( postResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( postResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( postResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_NotSameMessageId( postRequest, postResponse );
                CoApTestAsserts.Assert_SameToken       ( postRequest, postResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version      ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type         ( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code         ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Valid );
                CoApTestAsserts.Assert_SameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken    ( request, response1 );
                CoApTestAsserts.Assert_EmptyPayload ( response1.Payload );
                
                CoApTestAsserts.Assert_Statistics( m_client.Statistics          , desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer.Statistics    , desiredOriginServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult POST__ProxyDelayed( )
        {
            Log.Comment( "*** Send one POST request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent             = 3,
                AcksReceived             = 3,
                DelayedResponsesReceived = 3,
                AcksSent                 = 3,
            };
            var desiredLocalServerStats = new Statistics()
            {
                CacheMisses          = 5,
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };
            var desiredOriginServerStats = new Statistics()
            {
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };
            
            ClearCachesAndStatistics( );

            var originResource = TestConstants.Resource__EchoQuery_Delayed;
            var proxyResource  = TestConstants.Resource__ProxyMoniker__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, proxyResource );

                //
                // Fetch default response (query)
                //
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
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( originResource.Path ), response.Payload );

                //
                // POST a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new CoAPServerUri( TestConstants.Server__LocalOriginEndPoint__8080, proxyResource.Path + "?echo=" + echoString );

                var postRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.POST )
                    .WithOptions    ( newUri.Options )
                    .WithPayload    ( MessagePayload_String.New( echoString ) )
                    .Build( );

                var postResponse = m_client.MakeRequest( postRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version         ( postResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( postResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( postResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_NotSameMessageId( postRequest, postResponse );
                CoApTestAsserts.Assert_SameToken       ( postRequest, postResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version          ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type             ( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code             ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId ( request, response1 );
                CoApTestAsserts.Assert_SameToken        ( request, response1 );
                CoApTestAsserts.Assert_Payload          ( Defaults.Encoding.GetBytes( echoString ), response1.Payload ); 
                
                CoApTestAsserts.Assert_Statistics( m_client          .Statistics, desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer    .Statistics, desiredOriginServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult PUT__ProxyImmediate( )
        {
            Log.Comment( "*** Send one PUT request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                AcksReceived              = 2,
                AcksSent                  = 2,
                DelayedResponsesReceived  = 2,
                ImmediateResposesReceived = 1,
            };
            var desiredLocalServerStats = new Statistics()
            {
                RequestsReceived            = 3,
                ImmediateResposesSent       = 1,
                DelayedResponsesSent        = 2,
                AcksSent                    = 2,
                AcksReceived                = 2,
                CacheHits                   = 1,
                CacheMisses                 = 3,
            };
            var desiredOriginServerStats = new Statistics()
            {
                RequestsReceived            = 2,
                ImmediateResposesSent       = 2,
            };


            ClearCachesAndStatistics( );

            var originResource = TestConstants.Resource__EchoQuery_Immediate;
            var proxyResource  = TestConstants.Resource__ProxyMoniker__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, proxyResource );

                //
                // Fetch default response (query)
                //
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

                CoApTestAsserts.Assert_Version         ( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken       ( request, response );

                //
                // PUT a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new CoAPServerUri( TestConstants.Server__LocalOriginEndPoint__8080, proxyResource.Path + "?echo=" + echoString );

                var putRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.PUT )
                    .WithOptions    ( newUri.Options )
                    .WithPayload    ( MessagePayload_String.New( echoString ) )
                    .Build( );

                var putResponse = m_client.MakeRequest( putRequest );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version         ( putResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type            ( putResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code            ( putResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Changed );
                CoApTestAsserts.Assert_NotSameMessageId( putRequest, putResponse );
                CoApTestAsserts.Assert_SameToken       ( putRequest, putResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version      ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type         ( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code         ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Valid );
                CoApTestAsserts.Assert_SameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken    ( request, response1 );
                CoApTestAsserts.Assert_EmptyPayload ( response1.Payload );
                
                CoApTestAsserts.Assert_Statistics( m_client.Statistics          , desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer.Statistics    , desiredOriginServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult PUT__ProxyDelayed( )
        {
            Log.Comment( "*** Send one PUT request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent             = 3,
                AcksReceived             = 3,
                DelayedResponsesReceived = 3,
                AcksSent                 = 3,
            };
            var desiredLocalServerStats = new Statistics()
            {
                CacheMisses          = 5,
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };
            var desiredOriginServerStats = new Statistics()
            {
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };

            ClearCachesAndStatistics( );

            var originResource = TestConstants.Resource__EchoQuery_Delayed;
            var proxyResource  = TestConstants.Resource__ProxyMoniker__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, proxyResource );

                //
                // Fetch default response (query)
                //
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

                CoApTestAsserts.Assert_Version          ( response, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type             ( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code             ( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId ( request, response );
                CoApTestAsserts.Assert_SameToken        ( request, response );

                //
                // PUT a new response (query)
                //

                var echoString = "my other new echo string";

                var newUri = new CoAPServerUri( TestConstants.Server__LocalOriginEndPoint__8080, proxyResource.Path + "?echo=" + echoString );

                var putRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.PUT )
                    .WithOptions    ( newUri.Options )
                    .WithPayload    ( MessagePayload_String.New( echoString ) )
                    .Build( );
                
                var putResponse = m_client.MakeRequest( putRequest );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version          ( putResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type             ( putResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code             ( putResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Changed );
                CoApTestAsserts.Assert_NotSameMessageId ( putRequest, putResponse );
                CoApTestAsserts.Assert_SameToken        ( putRequest, putResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(response1 == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version          ( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type             ( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code             ( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId ( request, response1 );
                CoApTestAsserts.Assert_SameToken        ( request, response1 );
                CoApTestAsserts.Assert_Payload          ( Defaults.Encoding.GetBytes( echoString ), response1.Payload );
                
                CoApTestAsserts.Assert_Statistics( m_client.Statistics          , desiredClientStats      , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_localProxyServer.Statistics, desiredLocalServerStats , TransmissionParameters.default_EXCHANGE_LIFETIME );
                CoApTestAsserts.Assert_Statistics( m_originServer.Statistics    , desiredOriginServerStats, TransmissionParameters.default_EXCHANGE_LIFETIME );
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
        public TestResult DELETE__ProxyImmediate( )
        {
            return TestResult.Pass;
        }


        [TestMethod]
        public TestResult DELETE__proxyDelayed( )
        {
            return TestResult.Pass;
        }
    }
}
