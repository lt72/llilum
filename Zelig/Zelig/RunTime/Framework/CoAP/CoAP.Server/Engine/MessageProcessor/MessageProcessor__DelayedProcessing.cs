//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_DelayedProcessing : ProcessingState
        {
            private ProcessingState_DelayedProcessing( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_DelayedProcessing( );
            }

            //
            // Helper methods
            // 

            public override void Process( )
            {
                var processor = this.Processor;

                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.MessageInflated;
                var queries    = msg.Options.Queries;

                //
                // Send Ack
                //

                processor.Engine.Owner.Statistics.AcksSent++;

                var response = this.Processor.MessageBuilder.CreateAck( messageCtx ).BuildAndReset( );

                Logger.Instance.Log( string.Format( $"<==(S)== Sending ACK response to {messageCtx.Source}: '{response}'" ) );

                messageCtx.Channel.Send( response.Buffer, 0, response.Buffer.Length, messageCtx.Source );

                //
                // Query resource
                // 
                m_processor = (MessageProcessor)processor;

                messageCtx.ResourceHandler.ExecuteMethod( msg.DetailCode_Request, queries[ 0 ], this.ResultAvailableHandler );
            }

            //--//

            private void ResultAvailableHandler( object res, uint responseCode )
            {
                var messageCtx = m_processor.MessageContext;

                messageCtx.ResponseCode = responseCode;

                if(res != null && (res is string || res is int))
                {
                    messageCtx.ResponsePayload = Defaults.Encoding.GetBytes( res.ToString( ) );
                }

                Advance( ProcessingState.State.DelayedResponseAvailable );
            }
        }
    }
}
