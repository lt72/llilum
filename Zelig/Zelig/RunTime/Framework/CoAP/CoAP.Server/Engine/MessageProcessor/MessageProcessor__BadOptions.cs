//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;

    public partial class MessageProcessor
    {
        internal class MessageProcessor__BadOptions : ProcessingState
        {
            private MessageProcessor__BadOptions( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new MessageProcessor__BadOptions( );
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

                if(msg.IsConfirmable)
                {
                    //
                    // Unrecognized options of class "critical" that occur in a Confirmable 
                    // request MUST cause the return of a 4.02( Bad Option ) response. 
                    // This response SHOULD include a diagnostic payload describing the 
                    // unrecognized option( s ) ( see Section 5.5.2 ).
                    // 
                    uint responseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.BadRequest ); 
                    byte[] payload = null;
                    foreach(var opt in msg.Options.Options)
                    {
                        //
                        // TODO: Order matters!
                        //
                        if(opt.IsBad)
                        {
                            MessageOption_Opaque badOption = (MessageOption_Opaque)opt;
                                
                            responseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.BadOption );
                            payload      = (byte[ ])badOption.Value;
                        }
                        else if(opt.IsNotAcceptable)
                        {
                            responseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.NotAcceptable );
                        }
                    }

                    messageCtx.ResponseCode    = responseCode;
                    messageCtx.ResponsePayload = payload;

                    Advance( ProcessingState.State.ImmediateResponseAvailable );
                }
                else
                {
                    //
                    // Unrecognized options of class "critical" that occur in a 
                    // Non-confirmable message MUST cause the message to be rejected.
                    //
                    Advance( ProcessingState.State.SendReset );
                }
            }
        }
    }
}
