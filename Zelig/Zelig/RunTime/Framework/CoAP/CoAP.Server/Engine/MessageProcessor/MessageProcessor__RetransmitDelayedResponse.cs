//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_RetransmitDelayedResponse : ProcessingState
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

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;

                processor.MessageEngine.Owner.Statistics.DelayedResposesRetransmissions++;
                
                var response = messageCtx.ResponseAwaitingAck;

                //Debug.Assert( messageCtx.Message .Type == response.Type                       );
                //Debug.Assert( messageCtx.Response.Type == CoAPMessage.MessageType.Confirmable );

                Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Re-sending DELAYED response with Message Id '{response.MessageId}' to {messageCtx.Source}." ) );

                this.Processor.MessageEngine.SendMessageAsync( response );

                Advance( ProcessingState.State.AwaitingAck );
            }
        }
    }
}
