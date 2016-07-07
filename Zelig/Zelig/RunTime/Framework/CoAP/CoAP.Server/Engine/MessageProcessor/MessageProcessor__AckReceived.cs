//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack.Abstractions;

    public partial class MessageProcessor
    {
        internal class ProcessingState__AckReceived : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.AcksReceived++;
                
                var messageCtx = processor.MessageContext;

                processor.StopTrackingAck( messageCtx );

                Advance( State.Archive );
            }
        }
    }
}
