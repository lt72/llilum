//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack.Abstractions
{
    using System;
    using CoAP.Common;
    using CoAP.Stack;

    public interface IResourceProvider
    {
        bool CanFetchImmediateResponse( CoAPMessage request );

        uint ExecuteMethod( CoAPMessage request, ref MessagePayload payload, ref MessageOptions options );

        bool IsReadOnly { get; } 

        bool IsProxy { get; } 
    }
}
