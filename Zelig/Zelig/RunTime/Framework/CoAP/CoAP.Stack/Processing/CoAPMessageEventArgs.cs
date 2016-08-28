//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    using System;
    using CoAP.Stack.Abstractions.Messaging;

    public class CoAPMessageEventArgs : EventArgs
    {
        public MessageContext MessageContext;

        public CoAPMessageEventArgs( MessageContext ctx )
        {
            this.MessageContext = ctx;
        }
    }

    //--//

    public delegate void CoAPMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args ); 
}
