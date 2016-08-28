//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_DelayedResponseAvailable : ProcessingState
        {
            private ProcessingState_DelayedResponseAvailable( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_DelayedResponseAvailable( );
            }

            //
            // Helper methods
            // 

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;

                processor.MessageEngine.Owner.Statistics.DelayedResponsesSent++;
                
                var response = this.Processor.MessageBuilder.CreateDelayedResponse( messageCtx.Message, messageCtx ).Build( );

                //Debug.Assert( messageCtx.Message.Type == response.Type );

                //
                // If the message is confirmable we need to send the response and start tracking the ACK atomically, so 
                // we will send the message in a different state. If it is not confirmable we just send the response.
                //
                if(messageCtx.Message.IsConfirmable)
                {
                    messageCtx.ResponseAwaitingAck = response;

                    Advance( ProcessingState.State.AwaitingAck );
                }
                else
                {
                    Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Sending NON DELAYED response to {messageCtx.Source}: '{response}'" ) );

                    this.Processor.MessageEngine.SendMessageAsync( response );

                    Advance( ProcessingState.State.Archive );
                }
            }
        }
    }
}
