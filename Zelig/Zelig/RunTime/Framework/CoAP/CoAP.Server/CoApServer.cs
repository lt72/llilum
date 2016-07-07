//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Collections.Generic;
    using System.Net;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public class CoAPServer : IServer
    {
        //
        // State
        // 
        
        private readonly MessageEngine                         m_messageEngine;
        private readonly IPEndPoint[]                          m_endPoints;
        private readonly Dictionary<string, IResourceProvider> m_resources;
        private          Statistics                            m_stats;

        //--//

        public CoAPServer( ServerCoAPUri uri, AsyncMessaging messaging, Statistics stats )
        {
            m_messageEngine = new MessageEngine( this, messaging ); 
            m_resources     = new Dictionary<string, IResourceProvider>( );
            m_endPoints     = uri.EndPoints;
            m_stats         = stats;
        }

        //
        // Helper methods
        //

        #region IServer

        public void Start( )
        {
            m_messageEngine.Start( ); 
        }

        public void Stop( )
        {
            m_messageEngine.Stop( );
        }

        public void AddProvider( string relativePath, IResourceProvider provider )
        {
            m_resources.Add( relativePath, provider );
        }

        public IResourceProvider QueryProvider( string relativePath )
        {
            IResourceProvider provider = null;

            //
            // Remove the query portion
            // 
            var path = relativePath.Split( new char[] { '?' } ); 

            if(m_resources.TryGetValue( path[0], out provider ))
            {
                return provider;
            }

            return null;
        }

        public IResourceHandler CreateResourceHandler( IResourceProvider provider )
        {
            return new ResourceHandler( provider );
        }

        //
        // Access methods
        //

        public IPEndPoint[ ] EndPoints
        {
            get
            {
                return m_endPoints;
            }
        }

        public Statistics Statistics
        {
            get
            {
                return m_stats;
            }
        }

        #endregion

        //--//

    }
}
