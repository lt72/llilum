//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState__AckReceived : ProcessingState
        {
            private ProcessingState__AckReceived( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState__AckReceived( );
            }

            //
            // Mhelper methods
            // 

            internal override void Process( )
            {
                var processor = this.Processor;

                processor.MessageEngine.Owner.Statistics.AcksReceived++;
                
                var messageCtx = processor.MessageContext;

                processor.StopAckTrackingTimer( messageCtx );

                Advance( State.Archive );
            }
        }
    }
}
