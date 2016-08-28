//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;


    public sealed class EchoProvider_Immediate : EchoProvider
    {
        //
        // State
        //

        //
        // Contructors
        //
        public EchoProvider_Immediate( ) : base( false, false )
        {
        }

        public override bool CanFetchImmediateResponse( CoAPMessage request )
        {
            return true;
        }
    }
}
