//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using CoAP.Common;

    public abstract class MessagePayload
    {
        public static readonly MessagePayload EmptyPayload = MessagePayload_Opaque.New( Constants.EmptyBuffer );

        //
        // State
        // 

        private readonly byte[] m_bytes;

        //--//

        //
        // Contructors
        //

        protected MessagePayload( byte[ ] payload )
        {
            m_bytes = payload == null ? Constants.EmptyBuffer : payload;
        }


        //
        // Helper methods
        // 

        public override bool Equals( object obj )
        {
            if(obj == null)
            {
                return false;
            }

            MessagePayload payload = obj as MessagePayload;

            if(payload == null)
            {
                return false;
            }

            return Utils.ByteArrayCompare( this.m_bytes, payload.m_bytes );
        }

        public override int GetHashCode( )
        {
            return (int)Utils.ByteArrayToHash( m_bytes );
        }

        public void Encode( NetworkOrderBinaryStream stream )
        {
            if(Object.ReferenceEquals( m_bytes, Constants.EmptyBuffer) == false)
            {
                stream.WriteByte( 0XFF ); 

                stream.WriteBytes( m_bytes, 0, m_bytes.Length );
            }
        }

        //
        // Helper methods
        // 

        public abstract object Value { get; }

        //
        // Access methods
        // 

        protected byte[ ] RawBytes
        {
            get
            {
                return m_bytes;
            }
        }

        public int Size
        {
            get
            {
                if(Object.ReferenceEquals( m_bytes, Constants.EmptyBuffer ))
                {
                    return 0;
                }

                return m_bytes.Length + 1; // account for marker
            }
        }

        //--//
    }
}