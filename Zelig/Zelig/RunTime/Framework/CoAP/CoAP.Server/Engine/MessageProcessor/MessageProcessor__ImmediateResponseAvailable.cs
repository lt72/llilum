//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Server
{
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_ImmediateResponseAvailable : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.ImmediateResposesSent++;

                var messageCtx = processor.MessageContext;

                var response = this.Processor.MessageBuilder.CreateAck( messageCtx )
                        .WithCode   ( messageCtx.ResponseCode    )
                        .WithPayload( messageCtx.ResponsePayload )
                        .BuildAndReset( );

                Logger.Instance.Log( string.Format( $"<==(S)== Sending IMMEDIATE (piggybacked) response to {messageCtx.Source}: '{response}'" ) );

                messageCtx.Channel.Send( response.Buffer, 0, response.Buffer.Length, messageCtx.Source );

                Advance( ProcessingState.State.Archive );
            }
        }
    }
}
