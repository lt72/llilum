//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    using System;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;


    public abstract class StandardResourceProvider : IResourceProvider
    {
        public abstract bool IsImmediate
        {
            get;
        }

        public abstract bool IsReadOnly
        {
            get;
        }

        public virtual uint ExecuteMethod( CoAPMessage.Detail_Request method, string query, out object res )
        {
            res = null;

            if(String.IsNullOrEmpty( query ))
            {
                return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.BadRequest );
            }

            switch(method)
            {
                case CoAPMessage.Detail_Request.GET:
                    return this.GET( query, out res );

                case CoAPMessage.Detail_Request.POST:
                    return this.POST( query );

                case CoAPMessage.Detail_Request.PUT:
                    return this.PUT( query );

                case CoAPMessage.Detail_Request.DELETE:
                    return this.DELETE( query );

                default:
                    return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
            }
        }

        //--//

        protected virtual uint GET( string query, out object result )
        {
            result = null;

            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint POST( string query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint PUT( string query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint DELETE( string query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }
    }
}
