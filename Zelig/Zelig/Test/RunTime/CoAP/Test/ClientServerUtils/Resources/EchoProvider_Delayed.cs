//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using CoAP.Stack.Abstractions;


    internal class EchoProvider_Delayed : EchoProvider
    {
        //
        // State
        //
        
        //
        // Contructors
        //
        internal EchoProvider_Delayed( )
        {
        }

        public override bool IsImmediate
        {
            get
            {
                return false;
            }
        }
    }
}
