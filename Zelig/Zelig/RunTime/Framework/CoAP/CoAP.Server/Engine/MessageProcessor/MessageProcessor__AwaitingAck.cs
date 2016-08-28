//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Server
{
    using CoAP.Common;
    using CoAP.Common.Diagnostics;

    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_AwaitingAck : ProcessingState
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

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;
                var id         = messageCtx.ResponseAwaitingAck.MessageId;

                //Debug.Assert( processor.Engine.IsAckPending( id ) == false );

                if(processor.MessageEngine.RegisterAckPending( messageCtx.ResponseAwaitingAck.Context, processor ))
                {
                    Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Sending CON DELAYED response to {messageCtx.Source}: '{messageCtx.ResponseAwaitingAck}'" ) );

                    processor.StartAckTrackingTimer( TransmissionParameters.InitialTimeout );

                    this.Processor.MessageEngine.SendMessageAsync( messageCtx.ResponseAwaitingAck );
                    
                    Advance( ProcessingState.State.Archive );
                }
            }

            //--//
        }
    }
}
