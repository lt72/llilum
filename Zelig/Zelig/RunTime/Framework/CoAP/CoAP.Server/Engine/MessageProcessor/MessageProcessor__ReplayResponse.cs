//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;

    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState__ReplayResponse : ProcessingState
        {
            //
            // State
            // 

            private readonly CoAPMessageRaw m_response;

            //
            // Constructors 
            //

            private ProcessingState__ReplayResponse( CoAPMessageRaw response )
            {
                m_response = response;
            }

            internal static ProcessingState Get( object state )
            {
                return new ProcessingState__ReplayResponse( (CoAPMessageRaw)state );
            }

            //
            // Helper methods
            // 

            internal override void Process( )
            {
                var processor = this.Processor;

                processor.MessageEngine.Owner.Statistics.ImmediateResposesSent++;
                
                var messageCtx = processor.MessageContext;

                Logger.Instance.Log( string.Format( $"<==[S({this.Processor.MessageEngine.LocalEndPoint})]== Sending IMMEDIATE (piggybacked) response to {messageCtx.Source}: '{m_response}'" ) );

                this.Processor.MessageEngine.Messaging.SendMessageAsync( m_response );
            }
        }
    }
}
