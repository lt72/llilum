//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;


    public class CoAP_TelemetryTagNotSupportException : NotSupportedException
    {
        public CoAP_TelemetryTagNotSupportException( ) : base( )
        {
        }

        public CoAP_TelemetryTagNotSupportException( string message ) : base( message )
        {
        }
    }
}

