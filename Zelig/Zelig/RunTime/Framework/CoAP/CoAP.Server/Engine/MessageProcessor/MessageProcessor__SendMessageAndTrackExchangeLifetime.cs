//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common;
    using CoAP.Common.Diagnostics;

    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_SendMessageAndTrackExchangeLifetime : ProcessingState
        {
            private ProcessingState_SendMessageAndTrackExchangeLifetime( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_SendMessageAndTrackExchangeLifetime( );
            }

            //
            // Helper methods
            // 

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;

                Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Sending ACK IMMEDIATE response to {messageCtx.Source}: '{messageCtx.ResponseAwaitingAck}'" ) );

                processor.StartExchangeLifeTimeTrackingTimer( TransmissionParameters.EXCHANGE_LIFETIME );

                this.Processor.MessageEngine.SendMessageAsync( messageCtx.ResponseAwaitingAck );
            }
        }
    }
}
