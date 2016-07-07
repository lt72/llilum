//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_ResetReceived : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.ResetsReceived++;

                var messageCtx = processor.MessageContext;

                Logger.Instance.LogWarning( $"==(S)==> Received RESET from {messageCtx.Source}: '{messageCtx.Message}'" );

                Advance( ProcessingState.State.Archive );
            }
        }
    }
}
