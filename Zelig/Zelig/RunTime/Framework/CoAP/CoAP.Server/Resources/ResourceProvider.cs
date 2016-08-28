//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Server
{
    using System;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;
    using CoAP.Stack;

    public abstract class ResourceProvider : IResourceProvider
    {
        //
        // State 

        private readonly bool m_isReadOnly;
        private readonly bool m_isProxy;

        //
        //
        // Contructor 
        //

        public ResourceProvider( bool isReadOnly, bool isProxy )
        {
            m_isReadOnly = isReadOnly;
            m_isProxy    = isProxy;
        }

        //
        // Helper methods
        // 

        public abstract bool CanFetchImmediateResponse( CoAPMessage request );
        
        public virtual uint ExecuteMethod( CoAPMessage request, ref MessagePayload payload, ref MessageOptions options )
        {
            var path    = request.Options.Path;
            var queries = request.Options.Queries;
            var method  = request.DetailCode_Request;

            if(String.IsNullOrEmpty( path ))
            {
                return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.BadRequest );
            }

            switch(method)
            {
                case CoAPMessage.Detail_Request.GET:
                    {
                        MessagePayload resource = null;
                        uint code = this.GET( path, queries, out resource );
                        
                        payload = resource;
                        options.Add( MessageOption_Opaque.New( MessageOption.OptionNumber.ETag, Utils.ByteArrayFromInteger( MessageBuilder.NewGlobalETag( ) ) ) );

                        return code;
                    }
                    
                case CoAPMessage.Detail_Request.POST:
                    return this.POST( path, queries );

                case CoAPMessage.Detail_Request.PUT:
                    return this.PUT( path, queries );

                case CoAPMessage.Detail_Request.DELETE:
                    return this.DELETE( path, queries );

                default:
                    return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
            }
        }
        
        protected virtual uint GET( string path, string[ ] query, out MessagePayload payload )
        {
            payload = null;

            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint POST( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint PUT( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        protected virtual uint DELETE( string path, string[ ] query )
        {
            return CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );
        }

        //
        // Access methods
        // 

        public bool IsReadOnly
        {
            get
            {
                return m_isReadOnly;
            }
        }

        public bool IsProxy
        {
            get
            {
                return m_isProxy;
            }
        }
    }
}
