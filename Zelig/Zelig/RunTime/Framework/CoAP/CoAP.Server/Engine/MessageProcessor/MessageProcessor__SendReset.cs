//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_SendReset : ProcessingState
        {
            private ProcessingState_SendReset( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_SendReset( );
            }

            //
            // Mhelper methods
            // 

            internal override void Process( )
            {
                var processor = this.Processor;

                processor.MessageEngine.Owner.Statistics.ResetsSent++;

                var messageCtx = processor.MessageContext;

                var response = this.Processor.MessageBuilder.CreateResetResponse( messageCtx.Message, messageCtx ).Build();
                            
                Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Sending RESET response to {messageCtx.Source}: '{response}'" ) );

                this.Processor.MessageEngine.SendMessageAsync( response );

                Advance( ProcessingState.State.Archive );
            }
        }
    }
}
