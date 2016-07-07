//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    public static class Constants
    {
        public static readonly int  MinimalMessageLength =    4;
        public static readonly byte PayloadMarker        = 0xFF;
        
        public static readonly byte[] EmptyBuffer = new byte[ 0 ];
    }
}
