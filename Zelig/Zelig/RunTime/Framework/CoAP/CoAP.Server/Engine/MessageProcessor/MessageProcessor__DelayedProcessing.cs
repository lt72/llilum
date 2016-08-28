//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_DelayedProcessing : ProcessingState
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

            internal override void Process( )
            {
                var processor = this.Processor;

                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.MessageInflated;

                //
                // Send Ack
                //

                processor.MessageEngine.Owner.Statistics.AcksSent++;

                var response = this.Processor.MessageBuilder.CreateAck( msg, messageCtx ).Build( );

                Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Tx ACK ID={response.MessageId} response to {messageCtx.Source}: '{response}'" ) );

                this.Processor.MessageEngine.SendMessageAsync( response );
                
                processor.ResourceHandler.ExecuteMethod( msg, this.DelayedResultAvailableCallback );
            }

            //--//

            private void DelayedResultAvailableCallback( uint responseCode, MessagePayload payload, MessageOptions options )
            {
                var messageCtx = this.Processor.MessageContext;

                messageCtx.ResponseCode    = responseCode;
                messageCtx.ResponsePayload = payload;

                messageCtx.ResponseOptions.Add( options );

                Advance( ProcessingState.State.DelayedResponseAvailable );
            }
        }
    }
}
