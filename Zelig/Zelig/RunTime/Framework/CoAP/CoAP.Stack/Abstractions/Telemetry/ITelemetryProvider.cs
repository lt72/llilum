//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public interface ITelemetryProvider
    {
        long CumulativeReadings { get; set; }
        
        long GetValue( string tag ); 
    }
}