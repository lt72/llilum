//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;


    public partial class AsyncMessageProcessor : MessageProcessor
    {
        //--//

        //
        // State
        // 

        //--//

        //
        // Constructors 
        // 

        public AsyncMessageProcessor( MessageContext node, ProcessingState state, MessageEngine owner ) : base( node, state, owner )
        {
        }

        public static AsyncMessageProcessor CreateMessageProcessor( MessageContext node, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.MessageReceived );
            var processor = new AsyncMessageProcessor( node, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        public static AsyncMessageProcessor CreateErrorProcessor( MessageContext node, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.Error );
            var processor = new AsyncMessageProcessor( node, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        public static AsyncMessageProcessor CreateOptionsErrorProcessor( MessageContext node, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.BadOptions );
            var processor = new AsyncMessageProcessor( node, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        //
        // Helper methods
        // 
        
        public override void Process( )
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
