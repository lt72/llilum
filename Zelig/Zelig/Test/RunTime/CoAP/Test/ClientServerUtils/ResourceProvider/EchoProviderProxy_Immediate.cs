//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using System.Net;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;
    using CoAP.Server;

    public sealed class EchoProviderProxy_Immediate : EchoProviderProxy
    {
        //
        // State
        //

        //
        // Contructors
        //

        public EchoProviderProxy_Immediate( CoAPServerUri uri, CoAPProxyServer proxyServices ) : base( uri, proxyServices )
        {
        }

        public void ClearLocalCache( )
        {
            this.LocalCache = null;
        }
    }
}
