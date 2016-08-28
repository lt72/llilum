//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;

    internal class ResourceCache
    {
        private class CacheKeys
        {
            public int ETagKey;
            public int SafeKey;

            internal CacheKeys()
            {
                ETagKey = -1;
                SafeKey = -1;
            }

            public override bool Equals( object obj )
            {
                if(obj == null)
                {
                    return false;
                }

                var keys = (CacheKeys)obj;

                //
                // A match is defined as one or the other key equality. 
                // 
                return this.ETagKey == keys.ETagKey || this.SafeKey == keys.SafeKey;
            }

            public override int GetHashCode( )
            {
                return this.ETagKey ^ this.SafeKey;
            }

            public override string ToString( )
            {
                return $"ETagKey={this.ETagKey},SafeKey={this.SafeKey}";
            }

            public static bool operator ==(CacheKeys keyA, CacheKeys keyB )
            {
                if((object)keyA == null && (object)keyB == null)
                {
                    return true;
                }

                if((object)keyA != null && (object)keyB != null)
                {
                    return keyA.Equals( keyB );
                }

                return false;
            }

            public static bool operator !=( CacheKeys keyA, CacheKeys keyB )
            {
                return !(keyA == keyB); 
            }
        }
        
        //--//

        //
        // State 
        // 

        private readonly Dictionary< CacheKeys, ResourceCacheEntry > m_store;
        private readonly LinkedList< CacheKeys                     > m_mru;
        private readonly int                                         m_cacheSizeThreshold;
        private          int                                         m_cacheSize;
        private readonly object                                      m_sync;

        //--//

        // 
        // Contructors
        //

        internal ResourceCache( int cacheSizeThreashold )
        {
            m_store              = new Dictionary<CacheKeys, ResourceCacheEntry>( );
            m_mru                = new LinkedList<CacheKeys>( );
            m_cacheSizeThreshold = cacheSizeThreashold;
            m_cacheSize          = 0;
            m_sync               = new object( );

            // add a dummy node for easy list maintanance, it will always be at the end of the list
            m_mru.AddFirst( new LinkedListNode<CacheKeys>( new CacheKeys( ) ) );
        }

        internal ResourceCache( ) : this( Defaults.ProxyCacheSizeThreshold )
        {
        }

        //
        // Helper methods
        // 

        internal bool TryGetValue( CoAPMessage request, IPEndPoint originEndPoint, out ResourceCacheEntry entry )
        {
            var keys = ComputeCacheKeys( request, originEndPoint );

            entry = null;

            lock(m_sync)
            {
                if(m_store.TryGetValue( keys, out entry ))
                {
                    if(IsFreshAndRelevant( entry, request.Options.ETag ))
                    {
                        UpdateMru( keys );

                        return true;
                    }
                }
            }

            return false;
        }

        internal void Refresh( CoAPMessage request, CoAPMessage response, ref ResourceCacheEntry entry )
        {
            var requestKeys  = ComputeCacheKeys( request , response.Context.Source );
            var responseKeys = ComputeCacheKeys( response, response.Context.Source );

            if(requestKeys != responseKeys)
            {
                Logger.Instance.LogWarning( $"Cache key for request ID={request.MessageId} changed from '{requestKeys}' to '{responseKeys}'" ); 
            }

            var expireTime = DateTime.Now + response.Options.MaxAge;
            
            //
            // From RFC7252: 
            //
            // A 2.03( Valid ) response indicates the stored response identified by 
            // the entity - tag given in the response’s ETag Option can be reused
            // after updating it as described in Section 5.9.1.3.
            // Any other Response Code indicates that none of the stored responses
            // nominated in the request is suitable. Instead, the response SHOULD
            // be used to satisfy the request and MAY replace the stored response.
            //
            lock(m_sync)
            {
                if(m_store.ContainsKey( responseKeys ))
                {
                    entry = m_store[ responseKeys ];

                    if(response.DetailCode_Success == CoAPMessageRaw.Detail_Success.Valid)
                    {
                        //
                        // Valid content, update the expire time and re-register the response 
                        // under the new key, computed from the current response options.
                        // In most cases should just match the previous key.
                        //

                        // 
                        // From RFC7252, section 5.9.1.3 - "2.03 Valid": 
                        // 
                        // This Response Code is related to HTTP 304 "Not Modified" but only
                        // used to indicate that the response identified by the entity-tag
                        // identified by the included ETag Option is valid.Accordingly, the
                        // response MUST include an ETag Option and MUST NOT include a payload.
                        // When a cache that recognizes and processes the ETag response option
                        // receives a 2.03( Valid ) response, it MUST update the stored response
                        // with the value of the Max-Age Option included in the response
                        // (explicitly, or implicitly as a default value; see also
                        // Section 5.6.2). For each type of Safe-to-Forward option present in
                        // the response, the (possibly empty) set of options of this type that
                        // are present in the stored response MUST be replaced with the set of
                        // options of this type in the response received.
                        // 

                        Debug.Assert( response.Payload.Equals( MessagePayload.EmptyPayload ) );
                    }
                    else
                    {
                        //
                        // Update payload as well as expire time 
                        // 

                        //
                        // From RFC7252, section 5.6.2 - "Validation Model":
                        //
                        // A 2.03( Valid ) response indicates the stored response identified by
                        // the entity-tag given in the response’s ETag Option can be reused
                        // after updating it as described in Section 5.9.1.3.
                        // Any other Response Code indicates that none of the stored responses
                        // nominated in the request is suitable.Instead, the response SHOULD
                        // be used to satisfy the request and MAY replace the stored response.
                        //

                        entry.ETag    = response.Options.ETag;
                        entry.Payload = response.Payload;
                    }

                    entry.ExpireTime = expireTime;

                    UpdateMru( responseKeys );
                }
                else
                {
                    entry = new ResourceCacheEntry( response.Options.ETag, response.Payload, expireTime );

                    AddEntry( responseKeys, entry );
                    AddMru  ( responseKeys        );
                }
            }
        }

        internal void Evict( CoAPMessage request, IPEndPoint originEndPoint )
        {
            var key = ComputeCacheKeys( request, originEndPoint );

            lock(m_sync)
            {
                m_store.Remove( key );
            }
        }

        internal void Clear()
        {
            lock(m_sync)
            {
                m_store.Clear( );
            }
        }

        //--//

        internal static bool IsFreshAndRelevant( ResourceCacheEntry entry, MessageOption ETag )
        {
            if(entry != null)
            {
                if(entry.ExpireTime >= DateTime.Now)
                {
                    //
                    // Match ETag only when client requests it.
                    //
                    return ETag == null || entry.ETag == ETag;
                }
            }

            return false;
        }

        private void AddEntry( CacheKeys keys, ResourceCacheEntry entry )
        {
            var size = entry.Payload.Size;

            if(size + m_cacheSize > m_cacheSizeThreshold)
            {
                Trim( ); 
            }

            m_store.Add( keys, entry );

            m_cacheSize += size;
        }

        private void AddMru( CacheKeys key )
        {
            m_mru.AddFirst( key );
        }

        private void UpdateMru( CacheKeys key )
        {
            var node = m_mru.Find( key );

            node.List.Remove( node );

            m_mru.AddFirst( node );
        }

        private void Trim( )
        {
            //
            // Halve the cache size, or try and keep just the last two entries
            // 

            // must have more nodes than just the dummy node...
            Debug.Assert( m_mru.Count >= 2 );

            var node = m_mru.Last.Previous;

            while(node != null && m_mru.Count>= 2 && m_cacheSize > m_cacheSizeThreshold)
            {
                var entry = m_store[ node.Value ];

                m_store.Remove( node.Value );

                m_cacheSize -= entry.Payload.Size;
                
                var previous = node.Previous;

                m_mru.Remove( node );

                node = previous;
            }

            Debug.Assert( m_mru.Count >= 2 );
        }

        //--//

        private static CacheKeys ComputeCacheKeys( CoAPMessage msg, IPEndPoint originEndPoint )
        {
            var keys = new CacheKeys(); 

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!! Cache key are considered unique per origin endpoint, not per provider !!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            var origin = originEndPoint.GetHashCode( );

            //
            // If an ETag is available,then that is all we need to use
            // TODO: this will have to change when we implement Location-*
            // options. 
            // 
            if(msg.IsTagged)
            {
                keys.ETagKey = msg.Options.ETag.GetHashCode( ) ^ origin; 
            }

            //
            // No ETag, compute the cache key from all available parameters. 
            // 
            
            int key = origin ^ 363177381; // 0x15A5A5A5, 0b10101101001011010010110100101
            foreach(var opt in msg.Options.Options)
            {
                if(opt.IsSafeToForward)
                {
                    key ^= opt.GetHashCode( );
                }
            }

            keys.SafeKey = key;

            return keys;
        }
    }
}