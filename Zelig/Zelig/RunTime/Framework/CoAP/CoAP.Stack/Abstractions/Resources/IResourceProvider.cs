//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public interface IResourceProvider
    {
        bool IsImmediate { get; }

        bool IsReadOnly { get; }

        uint ExecuteMethod( CoAPMessage.Detail_Request method, string query, out object result );        
    }
}
