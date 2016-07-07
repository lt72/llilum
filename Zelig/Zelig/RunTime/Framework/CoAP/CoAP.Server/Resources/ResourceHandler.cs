//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using CoAP.Stack.Abstractions;
    using System.Threading;
    using Stack;

    public class ResourceHandler : IResourceHandler
    {

        //
        // State 
        // 

        private readonly IResourceProvider m_provider;

        //--//

        //
        // Contructors Methods
        // 

        public ResourceHandler( IResourceProvider provider )
        {
            m_provider = provider;
        }

        //
        // Helper Methods
        // 

        #region IResourceHandler 

        public void ExecuteMethod( CoAPMessage.Detail_Request method, string query, ResultAvailable handler )
        {
            if(query == null || handler == null)
            {
                throw new ArgumentException( );
            }

            ThreadPool.QueueUserWorkItem( ( o ) =>
            {
                //
                // Simply forward to provider
                // 
                object res = null;
                var responseCode = m_provider.ExecuteMethod( method, query, out res );

                handler( res, responseCode );

            }, null );
        }

        #endregion

        //--//
        
    }
}
