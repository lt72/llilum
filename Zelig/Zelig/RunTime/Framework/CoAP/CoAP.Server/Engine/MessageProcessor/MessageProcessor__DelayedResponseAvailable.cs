//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_DelayedResponseAvailable : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.DelayedResponsesSent++;

                var messageCtx = processor.MessageContext;
                
                var response = this.Processor.MessageBuilder.CreateDelayedResponse( messageCtx )
                    .WithPayload( messageCtx.ResponsePayload )
                    .BuildAndReset( );

                //Debug.Assert( messageCtx.Message.Type == response.Type );

                Logger.Instance.Log( string.Format( $"<==(S)== Sending DELAYED response to {messageCtx.Source}: '{response}'" ) );

                messageCtx.Channel.Send( response.Buffer, 0, response.Buffer.Length, messageCtx.Source );

                if(messageCtx.MessageInflated.IsConfirmable)
                {
                    messageCtx.Response = response;

                    Advance( ProcessingState.State.AwaitingAck );
                }
                else
                {
                    Advance( ProcessingState.State.Archive );
                }
            }
        }
    }
}
