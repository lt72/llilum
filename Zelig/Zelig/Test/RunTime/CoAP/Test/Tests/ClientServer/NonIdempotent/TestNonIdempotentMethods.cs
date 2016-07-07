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


    public class TestNonIdempotentMethods : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;

            //res |= POST__Immediate  ( );
            //res |= POST__Delayed    ( );
            res |= PUT__Immediate   ( );
            res |= PUT__Delayed     ( );
            res |= DELETE__Immediate( );
            res |= DELETE__Delayed  ( );

            return res;
        }

        [TestMethod]
        public TestResult POST__Immediate( )
        {
            Log.Comment( "*** Send one POST request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                ImmediateResposesReceived = 3,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived      = 3,
                ImmediateResposesSent = 3,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // Fetch default response (query)
                //
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
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( resource.Path ), response );

                //
                // POST a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new ServerCoAPUri( TestConstants.EndPoint__8080, resource.Path + "?echo=" + echoString );
                
                var postRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.POST )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, newUri.Path ) )
                    .WithPayload    ( Defaults.Encoding.GetBytes( echoString ) )
                    .BuildAndReset( );
                
                var postResponse = m_client.MakeRequest( postRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( postResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( postResponse, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( postResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_SameMessageId( postRequest, postResponse );
                CoApTestAsserts.Assert_SameToken( postRequest, postResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( echoString ), response1 );



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
        public TestResult POST__Delayed( )
        {
            Log.Comment( "*** Send one POST request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                AcksReceived              = 3,
                DelayedResposesesReceived = 3,
                AcksSent                  = 3,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // Fetch default response (query)
                //
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
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( resource.Path ), response );

                //
                // POST a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new ServerCoAPUri( TestConstants.EndPoint__8080, resource.Path + "?echo=" + echoString );

                var postRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.POST )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, newUri.Path ) )
                    .WithPayload    ( Defaults.Encoding.GetBytes( echoString ) )
                    .BuildAndReset( );

                var postResponse = m_client.MakeRequest( postRequest );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( postResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( postResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( postResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_NotSameMessageId( postRequest, postResponse );
                CoApTestAsserts.Assert_SameToken( postRequest, postResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(postResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( echoString ), response1 );



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
        public TestResult PUT__Immediate( )
        {
            Log.Comment( "*** Send one PUT request for a known IMMEDIATE and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                ImmediateResposesReceived = 3,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived      = 3,
                ImmediateResposesSent = 3,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Immediate;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // Fetch default response (query)
                //
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
                CoApTestAsserts.Assert_SameMessageId( request, response );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( resource.Path ), response );

                //
                // PUT a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new ServerCoAPUri( TestConstants.EndPoint__8080, resource.Path + "?echo=" + echoString );

                var putRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.PUT )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, newUri.Path ) )
                    .WithPayload    ( Defaults.Encoding.GetBytes( echoString ) )
                    .BuildAndReset( );

                var putResponse = m_client.MakeRequest( putRequest );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( putResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( putResponse, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( putResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_SameMessageId( putRequest, putResponse );
                CoApTestAsserts.Assert_SameToken( putRequest, putResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Acknowledgement );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_SameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( echoString ), response1 );



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
        public TestResult PUT__Delayed( )
        {
            Log.Comment( "*** Send one PUT request for a known DELAYED and CONFIRMABLE resource and verify that resource and answer code is as expected" );

            var desiredClientStats = new Statistics()
            {
                RequestsSent              = 3,
                AcksReceived              = 3,
                DelayedResposesesReceived = 3,
                AcksSent                  = 3,
            };
            var desiredServerStats = new Statistics()
            {
                RequestsReceived     = 3,
                AcksReceived         = 3,
                DelayedResponsesSent = 3,
                AcksSent             = 3,
            };

            ClearStatistics( );

            var resource = TestConstants.Resource__EchoQuery_Delayed;

            try
            {
                var messageBuilder = m_client.Connect( null, resource );

                //
                // Fetch default response (query)
                //
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
                CoApTestAsserts.Assert_Type( response, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_SameToken( request, response );
                CoApTestAsserts.Assert_NotSameMessageId( request, response );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( resource.Path ), response );

                //
                // PUT a new response (query)
                //

                var echoString = "my new echo string";

                var newUri = new ServerCoAPUri( TestConstants.EndPoint__8080, resource.Path + "?echo=" + echoString );

                var putRequest = messageBuilder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.PUT )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, newUri.Path ) )
                    .WithPayload    ( Defaults.Encoding.GetBytes( echoString ) )
                    .BuildAndReset( );
                
                var putResponse = m_client.MakeRequest( putRequest );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( putResponse, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( putResponse, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( putResponse, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Created );
                CoApTestAsserts.Assert_NotSameMessageId( putRequest, putResponse );
                CoApTestAsserts.Assert_SameToken( putRequest, putResponse );

                //
                // Verify new response 
                //

                var response1 = m_client.MakeRequest( request );

                if(putResponse == null)
                {
                    Log.Comment( "*** COMPLETED: FAIL" );
                    return TestResult.Fail;
                }

                CoApTestAsserts.Assert_Version( response1, CoAPMessage.ProtocolVersion.Version_1 );
                CoApTestAsserts.Assert_Type( response1, CoAPMessage.MessageType.Confirmable );
                CoApTestAsserts.Assert_Code( response1, CoAPMessage.Class.Success, CoAPMessage.Detail_Success.Content );
                CoApTestAsserts.Assert_NotSameMessageId( request, response1 );
                CoApTestAsserts.Assert_SameToken( request, response1 );
                CoApTestAsserts.Assert_Payload( Defaults.Encoding.GetBytes( echoString ), response1 );



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
        public TestResult DELETE__Immediate( )
        {
            return TestResult.Pass;
        }


        [TestMethod]
        public TestResult DELETE__Delayed( )
        {
            return TestResult.Pass;
        }
    }
}
