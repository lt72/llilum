//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    using System.Text;


    public static class Defaults
    {
        public static readonly Encoding Encoding             = Encoding.UTF8;
        public static readonly int      TokenLength          = 8;
        public static readonly int      MaxActiveConnections = 1;
    }
}
