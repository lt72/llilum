//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public delegate void ResultAvailable( object result, uint responseCode ); 

    public interface IResourceHandler
    {
        void ExecuteMethod( CoAPMessage.Detail_Request method, string query, ResultAvailable handler );
    }
}