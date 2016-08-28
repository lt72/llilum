//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Threading;
    using CoAP.Stack;


    internal sealed class AsyncMessageProcessor : MessageProcessor
    {
        //--//

        //
        // State
        // 

        //--//

        //
        // Constructors 
        // 

        public AsyncMessageProcessor( MessageContext ctx, ProcessingState state, MessageEngine owner ) : base( ctx, state, owner )
        {
        }

        public static AsyncMessageProcessor CreateMessageProcessor( MessageContext ctx, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.MessageReceived );
            var processor = new AsyncMessageProcessor( ctx, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        public static AsyncMessageProcessor CreateErrorProcessor( MessageContext ctx, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.Error );
            var processor = new AsyncMessageProcessor( ctx, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        public static AsyncMessageProcessor CreateOptionsErrorProcessor( MessageContext ctx, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.BadOptions );
            var processor = new AsyncMessageProcessor( ctx, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        //
        // Helper methods
        // 

        internal override void Process( )
        {
            ThreadPool.QueueUserWorkItem( o => 
                {
                try
                {
                    m_state.Process( );
                }
                catch
                {
                    // TODO: what logging?
                }
            }, null );
        }
        
        //--//

    }
}
