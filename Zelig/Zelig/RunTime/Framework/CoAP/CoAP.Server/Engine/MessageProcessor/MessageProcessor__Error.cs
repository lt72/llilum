//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;

    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_Error : ProcessingState
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

            internal override void Process( )
            {
                var processor = this.Processor;

                processor.MessageEngine.Owner.Statistics.Errors++;

                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.Message;
                var error      = messageCtx.ProtocolError;

                if(error == CoAPMessageRaw.Error.Processing__AckNotReceived)
                {
                    //
                    // TODO: implement better handling of missed acks and ignored messages
                    // BUG BUG BUG: TEST
                    //

                    Logger.Instance.LogProtocolError( $"ACK for MessageID={msg.MessageId} was not received! (error={messageCtx.ResponseCode})" ); 

                    Advance( ProcessingState.State.Archive );
                }
                else
                {
                    //
                    // Response code already set by previous state...
                    // 
                    if(msg.IsConfirmable)
                    {
                        Logger.Instance.LogProtocolError( $"ERROR processing CON MessageID={msg.MessageId}, sending response with code={messageCtx.ResponseCode}" );

                        Advance( ProcessingState.State.ImmediateResponseAvailable );
                    }
                    else
                    {
                        //
                        // NON messages are simply ignored
                        //  BUG BUG BUG: TEST
                        //
                        Logger.Instance.LogProtocolError( $"ERROR processing NON MessageID={msg.MessageId} (error={messageCtx.ResponseCode}). Dropping..." );

                        Advance( ProcessingState.State.Archive );
                    }
                }
            }
        }
    }
}
