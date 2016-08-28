//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_ResetReceived : ProcessingState
        {
            private ProcessingState_ResetReceived( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_ResetReceived( );
            }

            //
            // Mhelper methods
            // 

            internal override void Process( )
            {
                var processor = this.Processor;

                processor.MessageEngine.Owner.Statistics.ResetsReceived++;

                var messageCtx = processor.MessageContext;

                Logger.Instance.LogWarning( $"==[S({this.Processor.MessageEngine.LocalEndPoint})]==> Rx RESET ID={messageCtx.Message.MessageId} from {messageCtx.Source}: '{messageCtx.Message}'" );

                Advance( ProcessingState.State.Archive );
            }
        }
    }
}
