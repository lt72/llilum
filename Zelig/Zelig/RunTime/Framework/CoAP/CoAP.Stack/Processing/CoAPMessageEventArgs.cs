//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    using System;

    public class CoAPMessageEventArgs : EventArgs
    {
        public MessageContext MessageContext;
    }

    //--//

    public delegate void CoAPMessageHandler( object sender, CoAPMessageEventArgs args ); 
}
