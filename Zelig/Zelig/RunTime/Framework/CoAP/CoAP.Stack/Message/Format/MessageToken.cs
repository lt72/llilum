//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System.Collections.Generic;
    using System.Text;
    using CoAP.Stack;
    using CoAP.Common;

    public class MessageToken : IEncodable
    {
        public class TokenComparer : IEqualityComparer<MessageToken>
        {
            public bool Equals( MessageToken x, MessageToken y )
            {
                return x.Equals( y );
            }

            public int GetHashCode( MessageToken token )
            {
                return token.GetHashCode( ); 
            }
        }

        //--//

        internal static readonly MessageToken EmptyToken = new MessageToken( Constants.EmptyBuffer );

        //
        // State
        //

        private readonly byte[] m_token;

        //--//

        //
        // Contructors
        //
        
        internal MessageToken( byte[] token )
        {
            m_token = token == null ? Constants.EmptyBuffer : token;
        }
        
        internal MessageToken( int length, MessageBuilder builder ) : this(  builder.NewToken( new byte[ length ] ) )
        {
        }

        //
        // Helper Methods
        //

        public void Encode( NetworkOrderBinaryStream stream )
        {
            stream.WriteBytes( m_token, 0, m_token.Length ); 
        }

        public override bool Equals( object obj )
        {
            if(obj == null || obj.GetType() != this.GetType())
            {
                return false; 
            }

            if(ReferenceEquals( this, obj ))
            {
                return true;
            }

            return Utils.ByteArrayCompare( this.m_token, ((MessageToken)obj).m_token ); 
        }
        
        public override int GetHashCode( )
        {
            int hash = (m_token.Length << 31);

            for(int i = 0; i < m_token.Length; i++)
            {
                hash |= m_token[ i ]; 
            }

            return hash;
        }

#if DESKTOP
        public override string ToString( )
        {
            var sb = new StringBuilder();

            for(int i = 0; i < m_token.Length; i++)
            {
                sb.Append( $"0x{m_token[ i ]:X}" ); 
                
                if(i < m_token.Length - 1)
                {
                    sb.Append( "," ); 
                }
            }

            return $"{sb}";
        }
#endif

        //--//

        //
        // Access Methods
        //
        
        public int Size
        {
            get
            {
                return m_token.Length;
            }
        }
    }
}

