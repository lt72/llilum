//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using CoAP.Stack.Abstractions;
    using System.Threading;
    using Stack;


    public class ResourceHandler
    {

        public delegate void ResultAvailable( uint responseCode, MessagePayload payload, MessageOptions options );

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

        public void ExecuteMethod( CoAPMessage request, ResultAvailable handler )
        {
            if(request == null || handler == null)
            {
                throw new ArgumentException( );
            }

            ThreadPool.QueueUserWorkItem( ( o ) =>
            {
                //
                // Simply forward to provider
                // 
                try
                {
                    MessagePayload payload = null;
                    MessageOptions options = new MessageOptions();

                    var responseCode = m_provider.ExecuteMethod( request, ref payload, ref options );

                    handler( responseCode, payload, options );
                }
                catch
                {
                    // TODO: what logging?
                }

            }, null );
        }

        public IResourceProvider Provider
        {
            get
            {
                return m_provider;
            }
        }

        #endregion

        //--//

    }
}
