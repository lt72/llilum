//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Client;
    using CoAP.Common.Diagnostics;

    public abstract class ResourceProviderProxy : ResourceProvider
    {
        //
        // State
        // 

        private readonly CoAPServerUri      m_originUri;
        private readonly CoAPProxyServer    m_server;
        private          ResourceCacheEntry m_data;
        private readonly object             m_dataSync;

        //--//

        //
        // Contructors
        //

        public ResourceProviderProxy( CoAPServerUri originUri, CoAPProxyServer server, bool isReadonly ) : base( isReadonly, true )
        {
            m_originUri      = originUri;
            m_server         = server;
            m_data           = null;
            m_dataSync       = new object( );
        }

        //
        // Helper methods
        // 

        public override bool CanFetchImmediateResponse( CoAPMessage request )
        {
            //
            // For GET request, an immediate response is available if the data is cached and valid. 
            // For any other requests, we need to use a delayed response.
            //
            if(!request.IsGET)
            {
                return false;
            }

            //
            // Check the local cache first and try refresh it if it has gone stale.
            //
            lock(m_dataSync)
            {
                if(ResourceCache.IsFreshAndRelevant( m_data, request.Options.ETag ))
                {
                    return true;
                }

                //
                // Invalidate local data and continue.
                //

                m_data = null;
            }

            //
            // In the current implementation, there is no need to fetch data from global cache 
            // because the local cache is refreshed when the server requests returns.
            // It could be done like below, and although this may be redudant, it still closes the window for 
            // race condition in a concurrent scenario.
            // 
            ResourceCacheEntry entry = null;
            if(m_server.TryGetCachedValue( request, m_originUri.EndPoints[ 0 ], out entry ))
            {
                //
                // Update local data.
                //

                m_data = entry;

                return true;
            }

            return false;
        }

        public override uint ExecuteMethod( CoAPMessage request, ref MessagePayload payload, ref MessageOptions options )
        {
            var ctx = request.Context;

            uint valid = CoAPMessageRaw.Success_WithDetail( CoAPMessageRaw.Detail_Success.Valid );

            //
            // Use local cache for GET only, it has been refreshed already if the server called 'CanFetchImmediateResponse'...
            //
            if(request.IsGET)
            {
                lock(m_dataSync)
                {
                    if(ResourceCache.IsFreshAndRelevant( m_data, request.Options.ETag ))
                    {
                        //
                        // Response is fresh, return ETag but not payload
                        // 
                        options.Add( m_data.ETag );

                        Logger.Instance.LogSuccess( $"CACHE HIT (ETag={request.Options.ETag}): Cache hit for request '{request.Options.Path}'" );

                        m_server.Statistics.CacheHits++;

                        return valid;
                    }
                }
            }

            Logger.Instance.LogWarning( $"CACHE MISS ETag=({request.Options.ETag}): Cache miss for request '{request.Options.Path}'" );

            m_server.Statistics.CacheMisses++;

            //
            // Use less retries and a shorter timeout, so eventually the client will bail out
            // before the proxy client will.
            //
            using(var client = new CoAPClient(
                m_server.Messaging,
                true,
                TransmissionParameters.InitialTimeout / 2 + 1,
                TransmissionParameters.MAX_RETRANSMIT / 2 + 1,
                new Statistics( ) ))
            {
                CoAPMessage originResponse = null;

                var builder = client.Connect( null, m_originUri );

                var originRequest = builder.CreateOriginRequest( request ).Build( );
                
                var messageCtx = MessageContext.WrapWithContext( originRequest );

                messageCtx.Source = client.RemoteEndPoint;

                originResponse = client.SendReceive( originRequest );

                if(originResponse != null)
                {
                    Logger.Instance.LogSuccess( $"PROXY: Transferred representation for request '{request.Options.Path}' with ETag=({originResponse.Options.ETag})" );

                    //
                    // Refresh cache and return result
                    // 
                    lock(m_dataSync)
                    {
                        m_server.RefreshCache( request, originResponse, ref m_data );

                        Debug.Assert( originResponse.Payload.Equals( m_data.Payload ) );

                        //
                        // Return ETag and payload as well.
                        // 
                        payload = m_data.Payload;

                        options.Add( m_data.ETag );
                    }

                    return originResponse.Code;
                }
                else
                {
                    Logger.Instance.LogError( $"PROXY: Error transferring representation for request '{request.Options.Path}'" );
                }
            }

            return CoAPMessage.ServerError_WithDetail( CoAPMessageRaw.Detail_ServerError.GatewayTimeout ); 
        }

        //
        // Access methods 
        // 

        protected CoAPServerUri OriginUri
        {
            get
            {
                return m_originUri;
            }
        }

        protected CoAPProxyServer Server
        {
            get
            {
                return m_server;
            }
        }

        protected ResourceCacheEntry LocalCache
        {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
            }
        }

        //--//

        protected override uint GET( string path, string[ ] query, out MessagePayload payload )
        {
            payload = null;

            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected override uint POST( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected override uint PUT( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected override uint DELETE( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }
    }
}
