﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Support.mbed
{
    using System.Runtime.InteropServices;

    //--//

    internal class UsTicker
    {
        [DllImport("C")]
        public static extern uint us_ticker_read();
    }
}
