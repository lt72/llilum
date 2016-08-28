//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using CoAP.Stack;


    public class ResourceCacheEntry
    {
        public MessageOption_Opaque ETag;
        public MessagePayload       Payload;
        public DateTime             ExpireTime;

        //
        // Constructors
        // 
        public ResourceCacheEntry( MessageOption_Opaque eTag, MessagePayload payload, DateTime expireTime )
        {
            this.ETag       = eTag;
            this.Payload    = payload;
            this.ExpireTime = expireTime;
        }

        //
        // Helper methods
        // 

        public override bool Equals( object obj )
        {
            if(obj == null)
            {
                return false;
            }

            var entry = obj as ResourceCacheEntry;

            if(obj == null)
            {
                return false;
            }

            return  this.ETag   .Equals( entry.ETag )         &&
                    this.Payload.Equals( entry.Payload.Value ) ;
        }

        public override int GetHashCode( )
        {
            return this.ETag.GetHashCode( ) ^ this.Payload.GetHashCode( ); 
        }
    }
}
