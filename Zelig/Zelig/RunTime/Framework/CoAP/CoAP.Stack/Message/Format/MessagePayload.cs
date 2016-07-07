//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Text;
    using CoAP.Common;


    public class MessagePayload
    {
        internal static readonly MessagePayload EmptyPayload = new MessagePayload( Constants.EmptyBuffer );

        //
        // State
        // 

        private readonly byte[] m_payload;

        //--//

        //
        // Contructors
        //

        internal MessagePayload( byte[ ] payload )
        {
            m_payload = payload == null ? Constants.EmptyBuffer : payload;
        }


        //
        // Helper methods
        // 

        public void Encode( NetworkOrderBinaryStream stream )
        {
            if(m_payload != Constants.EmptyBuffer)
            {
                stream.WriteByte( 0XFF ); 

                stream.WriteBytes( m_payload, 0, m_payload.Length );
            }
        }

#if DESKTOP
        public override string ToString( )
        {
            var sb = new StringBuilder();
            
            for(int i = 0; i < m_payload.Length; i++)
            {
                sb.Append( $"0x{m_payload[ i ]:X}" ); 
                
                if(i < m_payload.Length - 1)
                {
                    sb.Append( "," ); 
                }
            }

            return $"{sb}";
        }
#endif

        //
        // Access methods
        // 

        public byte[ ] Payload
        {
            get
            {
                return m_payload;
            }
        }

        public int Size
        {
            get
            {
                return Object.ReferenceEquals( m_payload, Constants.EmptyBuffer ) ? 0 : m_payload.Length + 1; // account for marker
            }
        }
    }
}