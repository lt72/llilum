//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_RetransmitDelayedResponse : ProcessingState
        {
            private ProcessingState_RetransmitDelayedResponse( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_RetransmitDelayedResponse( );
            }

            //
            // Helper methods
            // 

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.DelayedResposesRetransmissions++;

                var messageCtx = processor.MessageContext;
                
                var response = messageCtx.Response;

                //Debug.Assert( messageCtx.Message .Type == response.Type                       );
                //Debug.Assert( messageCtx.Response.Type == CoAPMessage.MessageType.Confirmable );

                Logger.Instance.Log( string.Format( $"<==(S)== Re-sending DELAYED response with Message Id '{response.MessageId}' to {messageCtx.Source}." ) );

                messageCtx.Channel.Send( response.Buffer, 0, response.Buffer.Length, messageCtx.Source );

                Advance( ProcessingState.State.AwaitingAck );
            }
        }
    }
}
