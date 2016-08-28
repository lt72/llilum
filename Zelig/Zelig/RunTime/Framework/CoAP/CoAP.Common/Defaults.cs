//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    using System;
    using System.Text;


    public static class Defaults
    {
        public static readonly Encoding Encoding                    = Encoding.UTF8;
        public static readonly int      TokenLength                 = 8;
        public static readonly int      MaxActiveConnections        = 1;
        public static readonly int      MaxAge_Seconds              = 60;
        public static readonly TimeSpan MaxAge                      = new TimeSpan( 0, 0, Defaults.MaxAge_Seconds );
        public static readonly string   ProxyDirectory              = "proxy";
        public static readonly string   ProxyDirectoryWithSeparator = "proxy/";
        public static readonly int      ProxyCacheSizeThreshold     = 2 * 1024; // 2 KB cache size
    }
}
