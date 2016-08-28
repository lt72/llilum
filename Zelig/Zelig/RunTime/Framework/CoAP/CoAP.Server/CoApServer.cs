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

    public class CoAPServer
    {
        //
        // State
        // 
        
        private readonly MessageEngine                         m_messageEngine;
        private readonly Dictionary<string, IResourceProvider> m_resourceProviders;
        private readonly Statistics                            m_stats;

        //--//

        internal CoAPServer( MessageEngine messageEngine, Statistics stats )
        {
            m_messageEngine     = messageEngine;
            m_resourceProviders = new Dictionary<string, IResourceProvider>( );
            m_stats             = stats;
        }

        //--//

        public static CoAPServer CreateServer( IPEndPoint[ ] endPoints, AsyncMessaging messaging )
        {
            var stats  = new Statistics( );
            var server = new CoAPServer( new MessageEngine( endPoints, messaging ), stats );

            server.Engine.SetOwner( server );

            return server;
        }

        //
        // Helper methods
        //

        #region CoAPServer

        public void Start( )
        {
            m_messageEngine.Start( ); 
        }

        public void Stop( )
        {
            m_messageEngine.Stop( );
        }

        public virtual bool AddProvider( CoAPServerUri uri, IResourceProvider provider )
        {
            foreach(var ep in uri.EndPoints)
            {
                if(EndPointsInSet( ep, m_messageEngine.OriginEndPoints ))
                {     
                    m_resourceProviders.Add( uri.Path, provider );

                    return true;
                }   
            }

            return false;
        }

        public IResourceProvider QueryProvider( string relativePath )
        {
            IResourceProvider provider = null;

            //
            // Remove the query portion
            // 
            var path = relativePath.Split( new char[] { '?' } );

            if(m_resourceProviders.TryGetValue( path[ 0 ], out provider ))
            {
                return provider;
            }

            return null;
        }

        #endregion
        
        #region IStatistics
        
        public Statistics Statistics
        {
            get
            {
                return m_stats;
            }
        }

        #endregion

        //
        // Access methods
        //

        internal MessageEngine Engine
        {
            get
            {
                return m_messageEngine;
            }
        }

        protected Dictionary<string, IResourceProvider> Providers
        {
            get
            {
                return m_resourceProviders;
            }
        }

        //--//

        protected static bool EndPointsInSet( IPEndPoint target, IPEndPoint[ ] set )
        {
            for(int i = 0; i < set.Length; i++)
            {
                if(set[ i ].Equals( target ))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
