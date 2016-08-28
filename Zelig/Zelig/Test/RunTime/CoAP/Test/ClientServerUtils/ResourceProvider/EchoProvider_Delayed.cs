//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Test.ClientServerUtils
{
    using CoAP.Stack;


    public sealed class EchoProvider_Delayed : EchoProvider
    {
        //
        // State
        //

        //
        // Contructors
        //

        public EchoProvider_Delayed( ) : base( false, false )
        {
        }

        public override bool CanFetchImmediateResponse( CoAPMessage request )
        {
            return false;
        }
    }
}
