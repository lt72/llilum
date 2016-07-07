//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    using System;
    using System.Net;
    using CoAP.Stack;
    using CoAP.Common.Diagnostics;

    public interface IServer
    {
        void Start( );

        void Stop( );

        void AddProvider( string relativePath, IResourceProvider provider );

        IResourceProvider QueryProvider( String path );

        IResourceHandler CreateResourceHandler( IResourceProvider provider );

        IPEndPoint[ ] EndPoints { get; }

        Statistics Statistics { get; }
    }
}
