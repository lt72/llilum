//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using CoAP.Common.Diagnostics;
    using CoAP.Stack.Abstractions;


    public partial class MessageProcessor
    {
        internal class ProcessingState_Archive : ProcessingState
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

            public override void Process( )
            {
                var processor = this.Processor;

                var node = processor.MessageContext;

                ((MessageEngine)processor.Engine).Deregister( node ); 

                Logger.Instance.Log( $">>(S)<<< Archiving transaction '{node.Message.MessageId}' from {node.Source}" );
            }
        }
    }
}
