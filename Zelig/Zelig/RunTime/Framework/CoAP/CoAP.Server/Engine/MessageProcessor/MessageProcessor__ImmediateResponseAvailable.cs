//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Server
{
    using CoAP.Common;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_ImmediateResponseAvailable : ProcessingState
        {
            private ProcessingState_ImmediateResponseAvailable( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_ImmediateResponseAvailable( );
            }

            //
            // Helper methods
            // 

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;

                processor.MessageEngine.Owner.Statistics.ImmediateResposesSent++;

                messageCtx.ResponseAwaitingAck = this.Processor.MessageBuilder.CreateImmediateResponse( messageCtx.Message, messageCtx ).Build( );
                
                Advance( ProcessingState.State.SendMessageAndTrackExchangeLifetime );
            }
        }
    }
}
