//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack;
    using CoAP.Common;

    internal abstract class EchoProvider : StandardResourceProvider
    {
        //
        // State
        //

        private string m_echo;
        
        //
        // Contructors
        //

        internal EchoProvider( )
        {
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        //--//

        protected override uint GET( string query, out object result )
        {
            //
            // Set string to echo
            //

            result = m_echo == null ? query : m_echo;

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }

        protected override uint POST( string query )
        {
            //
            // Set string to echo, SHOULD return the URI of the new resource in a sequence of one or more
            // Location-Path and/or Location-Query Options
            //

            var echo = m_echo;

            m_echo = query.Substring( query.IndexOf( "?echo=" ) + "?echo=".Length );
            
            if(String.IsNullOrEmpty( m_echo ))
            {
                return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Deleted );
            }
            else if(echo == null)
            {
                return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Created );
            }

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Changed );
        }

        protected override uint PUT( string query )
        {
            //
            // Update string to echo
            //
            
            var echo = m_echo;

            m_echo = query.Substring( query.IndexOf( "?echo=" ) + "?echo=".Length );

            if(echo == null && m_echo != null)
            {
                return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Created );
            }

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Changed );
        }

        protected override uint DELETE( string query )
        {
            //
            // Reset string to echo
            //

            m_echo = null;
            
            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Deleted );
        }
    }
}
