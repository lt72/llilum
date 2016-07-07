//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public abstract class AbstractTelemetryProvider : ITelemetryProvider
    {
        public const string CumulativeReadings_Tag = "cmltvrdngs";

        public long CumulativeReadings { get; set; }

        public abstract long GetValue( string tag );
    }
}
