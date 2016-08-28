//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;
    using CoAP.Server;

    public sealed class EchoProviderProxy_Delayed : EchoProviderProxy
    {
        //
        // State
        //

        //
        // Contructors
        //

        public EchoProviderProxy_Delayed( CoAPServerUri uri, CoAPProxyServer proxyServices ) : base( uri, proxyServices )
        {
        }

        //
        // Helper methods
        //
        
        public override uint ExecuteMethod( CoAPMessage request, ref MessagePayload payload, ref MessageOptions options )
        {
            uint result = base.ExecuteMethod( request, ref payload, ref options );

            //
            // Emulate a delayed response by trashing the cache every time it is populated.
            //
            this.Server.EvictCachedValue( request, this.OriginUri.EndPoints[ 0 ] );

            this.LocalCache = null;

            return result;
        }

        public void ClearLocalCache( )
        {
            this.LocalCache = null;
        }
    }
}
