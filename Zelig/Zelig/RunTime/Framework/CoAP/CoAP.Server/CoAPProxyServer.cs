//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Net;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;
    using System;

    public sealed class CoAPProxyServer : CoAPServer
    {
        //
        // State
        // 
        
        private readonly ResourceCache m_resourceCache;

        //--//

        private CoAPProxyServer( MessageEngineProxy messageEngine, Statistics stats ) : base( messageEngine, stats )
        {
            m_resourceCache  = new ResourceCache( );
        }

        //--//

        public static CoAPProxyServer CreateProxyServer( IPEndPoint[ ] endPoints, AsyncMessaging messaging )
        {
            var stats  = new Statistics( );
            var server = new CoAPProxyServer( new MessageEngineProxy( endPoints, messaging ), stats );

            server.Engine.SetOwner( server );

            return server;
        }

        //
        // Helper methods
        //

        #region CoAPServer

        public override bool AddProvider( CoAPServerUri uri, IResourceProvider provider )
        {
            if(base.AddProvider( uri, provider ) == false)
            {
                //
                // If a provider does not match the set of origin endpoints for Local Server, then try and add it as a proxy.
                // 

                foreach(var ep in uri.EndPoints)
                {
                    if(EndPointsInSet( ep, ((MessageEngineProxy)this.Engine).ProxyEndPoints ) == false)
                    {
                        AddProxyEndPoint( ep );
                    }
                }

                this.Providers.Add( Defaults.ProxyDirectoryWithSeparator + uri.Path, provider );
            }

            return true;
        }

        #endregion

        #region CoAPProxyServer

        public AsyncMessaging Messaging
        {
            get
            {
                return this.Engine.Messaging;
            }
        }

        #endregion

        #region IResourceCache

        internal bool TryGetCachedValue( CoAPMessage request, IPEndPoint originUri, out ResourceCacheEntry entry )
        {
            bool fHit = m_resourceCache.TryGetValue( request, originUri, out entry );

            if(fHit)
            {
                Logger.Instance.LogSuccess( $"CACHE HIT ETag=({request.Options.ETag}): Cache hit for request '{request.Options.Path}'" );

                this.Statistics.CacheHits++;
            }
            else
            {
                Logger.Instance.LogWarning( $"CACHE MISS ETag=({request.Options.ETag}): Cache miss for request '{request.Options.Path}'" );

                this.Statistics.CacheMisses++;
            }

            return fHit;
        }

        internal void RefreshCache( CoAPMessage request, CoAPMessage response, ref ResourceCacheEntry entry )
        {
            m_resourceCache.Refresh( request, response, ref entry );
        }

        public void EvictCachedValue( CoAPMessage request, IPEndPoint originEndPoint )
        {
            m_resourceCache.Evict( request, originEndPoint );
        }

        public void EmptyCache( )
        {
            m_resourceCache.Clear( );
        }

        #endregion

        //
        // Access Methods
        //

        //--//
        
        private void AddProxyEndPoint( IPEndPoint endPoint )
        {
            var currentProxyEndPoints = ((MessageEngineProxy)this.Engine).ProxyEndPoints;
            int length                = currentProxyEndPoints.Length;

            var proxyEndPoints = new IPEndPoint[ length + 1 ];

            Array.Copy( currentProxyEndPoints, proxyEndPoints, currentProxyEndPoints.Length );

            proxyEndPoints[ length ] = endPoint;

            ((MessageEngineProxy)this.Engine).ProxyEndPoints = proxyEndPoints;
        }
    }
}
