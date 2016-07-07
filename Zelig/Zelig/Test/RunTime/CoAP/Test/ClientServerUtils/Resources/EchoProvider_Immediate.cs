//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using CoAP.Stack.Abstractions;


    internal class EchoProvider_Immediate : EchoProvider
    {
        //
        // State
        //
        
        //
        // Contructors
        //
        internal EchoProvider_Immediate( )
        {
        }

        public override bool IsImmediate
        {
            get
            {
                return true;
            }
        }
    }
}
