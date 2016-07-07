//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack.Abstractions;

    public partial class MessageProcessor
    {
        internal class ProcessingState_AwaitingAck : ProcessingState
        {
            private ProcessingState_AwaitingAck( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_AwaitingAck( );
            }

            //
            // Helper methods
            // 

            public override void Process( )
            {
                var processor = this.Processor;

                var messageCtx = processor.MessageContext;
                var id         = messageCtx.Response.MessageId;

                //Debug.Assert( processor.Engine.IsAckPending( id ) == false );

                processor.Engine.RegisterAckPending( id, processor ); 

                processor.StartTrackingAck( TransmissionParameters.InitialTimeout );
            }

            //--//
        }
    }
}
