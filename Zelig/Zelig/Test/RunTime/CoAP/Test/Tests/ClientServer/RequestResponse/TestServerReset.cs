//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using System.Threading;
    using Microsoft.Zelig.Test;
    using Test.ClientServerUtils;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;


    public class TestServerReset : CoApTestBase
    {
        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= ServerBogusResponseWrongmessageId( );

            return res;
        }

        private void ClearStatistics()
        {
            m_client.Statistics.Clear( );
            m_server.Statistics.Clear( );
        }

        private bool MessageMockHandler_ChangeMessageId( object sender, ref CoAPMessageEventArgs args )
        {
            //
            // Drop first message received
            // 
            int changedIds = m_server.MessagingMock.ChangedMessagesCount;

            if(Interlocked.Decrement( ref changedIds ) >= 0)
            {
                Logger.Instance.LogWarning( $"*** Request received '{args.MessageContext.Message}', changing message ID..." );
                
                var resource = TestConstants.Resource__EchoQuery_Immediate;

                var msg = MessageBuilder.Create( resource )
                    .FromContext( args.MessageContext )
                    .WithMessageId( (ushort)(args.MessageContext.Message.MessageId + 42) )
                    .Build( );

                CoAPMessage msgInflated = CoAPMessage.FromBuffer( msg.Buffer );

                using(var parser = MessageParser.CheckOutParser( ))
                {
                    parser.Inflate( msgInflated );
                }

                args.MessageContext.Message = msgInflated;

                Logger.Instance.LogWarning( $"*** New request '{args.MessageContext.Message}'" );

                m_server.MessagingMock.ChangedMessagesCount = changedIds; 
            }

            return true;
        }

        [TestMethod]
        public TestResult ServerBogusResponseWrongmessageId( )
        {
            Log.Comment( "*** Send one request for a known IMMEDIATE resource and get a bogus server response with wrong Message ID." );
            Log.Comment( "*** Verify client sends reset and server handles it correctly." );

            var desiredClientStats = new Statistics()
            {
                ResetsSent                  = 1,
                RequestsSent                = 1,
                RequestsRetransmissions     = 1, 
                ImmediateResposesReceived   = 1,
            };
            var desiredServerStats = new Statistics()
            {
                ResetsReceived          = 1,
                RequestsReceived        = 2,
                ImmediateResposesSent   = 2,
            };

            ClearStatistics( );

            try
            {
                //
                // Setup mock, undo at the end of test
                // 
                m_server.MessagingMock.OnMessageMock += MessageMockHandler_ChangeMessageId;

                m_server.MessagingMock.ChangedMessagesCount = 1;

                var resource = TestConstants.Resource__EchoQuery_Immediate;

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
                m_server.MessagingMock.OnMessageMock -= MessageMockHandler_ChangeMessageId;

                m_server.MessagingMock.ChangedMessagesCount = 0;

                m_client.Disconnect( ); 
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }
    }
}
