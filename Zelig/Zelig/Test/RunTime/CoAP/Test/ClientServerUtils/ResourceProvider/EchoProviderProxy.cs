//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;
    using CoAP.Server;
    using CoAP.Common;

    public abstract class EchoProviderProxy : ResourceProviderProxy
    {
        //
        // State
        //

        //////private string m_echo;

        //
        // Contructors
        //

        public EchoProviderProxy( CoAPServerUri uri, CoAPProxyServer proxyServices ) : base( uri, proxyServices, false )
        {
        }

        //--//

    }
}
