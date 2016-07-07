//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;

    public partial class MessageProcessor
    {
        internal class ProcessingState_Error : ProcessingState
        {
            private ProcessingState_Error( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_Error( );
            }

            //
            // Mhelper methods
            // 

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.Errors++;

                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.MessageInflated;
                var error      = messageCtx.Error;

                if(error == CoAPMessageRaw.Error.Processing__AckNotReceived)
                {
                    //
                    // TODO: implement better handling of missed acks and ignored messages
                    //
                    Advance( ProcessingState.State.Archive );
                }
                else
                {
                    //
                    // Response code already set by previous state...
                    // 
                    if(msg.IsConfirmable)
                    {
                        Advance( ProcessingState.State.ImmediateResponseAvailable );
                    }
                    else
                    {
                        //
                        // NON messages are simply ignored
                        // 
                        Advance( ProcessingState.State.Archive );
                    }
                }
            }
        }
    }
}
