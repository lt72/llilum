//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public partial class MessageProcessor
    {
        internal class ProcessingState_SendReset : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                processor.Engine.Owner.Statistics.ResetsSent++;

                var messageCtx = processor.MessageContext;

                var response = this.Processor.MessageBuilder.CreateResetResponse( messageCtx ).BuildAndReset();
                            
                Logger.Instance.Log( string.Format( $"<==(S)== Sending RESET response to {messageCtx.Source}: '{response}'" ) ); 

                messageCtx.Channel.Send( response.Buffer, 0, response.Buffer.Length, messageCtx.Source );

                Advance( ProcessingState.State.Archive );
            }
        }
    }
}
