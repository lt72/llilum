//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;


    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_Archive : ProcessingState
        {
            private ProcessingState_Archive( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_Archive( );
            }

            //
            // Mhelper methods
            // 

            internal override void Process( )
            {
                var processor  = this.Processor;
                var messageCtx = processor.MessageContext;

                ((MessageEngine)processor.MessageEngine).DeregisterLocalRequest( messageCtx );
                Logger.Instance.Log( $">>[S({processor.MessageEngine.LocalEndPoint})]<<< Archiving transaction '{messageCtx.Message.MessageId}' from {messageCtx.Source}" );
            }
        }
    }
}
