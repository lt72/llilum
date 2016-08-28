//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using CoAP.Stack;
    using CoAP.Server;

    public abstract class EchoProvider : ResourceProvider
    {
        //
        // State
        //

        private string m_echo;

        //
        // Contructors
        //

        public EchoProvider( bool isReadOnly, bool isProxy ) : base( isReadOnly, isProxy )
        {
        }

        //--//

        protected override uint GET( string path, string[] query, out MessagePayload payload )
        {
            //
            // Set string to echo
            //

            payload = MessagePayload_String.New( m_echo == null ? path : m_echo );

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }

        protected override uint POST( string path, string[] query )
        {
            //
            // Set string to echo, SHOULD return the URI of the new resource in a sequence of one or more
            // Location-Path and/or Location-Query Options
            //

            var echo = m_echo;

            m_echo = query[ 0 ].Substring( query[ 0 ].IndexOf( "echo=" ) + "echo=".Length );
            
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

        protected override uint PUT( string path, string[] query )
        {
            //
            // Update string to echo
            //
            
            var echo = m_echo;

            m_echo = query[ 0 ].Substring( query[ 0 ].IndexOf( "echo=" ) + "echo=".Length );

            if(echo == null && m_echo != null)
            {
                return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Created );
            }

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Changed );
        }

        protected override uint DELETE( string path, string[ ] query )
        {
            //
            // Reset string to echo
            //

            m_echo = null;

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Deleted );
        }
    }
}
