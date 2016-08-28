//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System;
    using System.Diagnostics;
    using System.Text;


    public class NetworkOrderBinaryStream
    {
        //
        // State
        //  

        private readonly byte[] m_buffer;
        private readonly int    m_last;
        private          int    m_current;

        //--//

        //
        // Contructors 
        // 
        
        internal NetworkOrderBinaryStream( byte[] buffer, int offset, int count )
        {
            if(buffer == null || buffer.Length < offset || buffer.Length < count)
            {
                throw new ArgumentException( ); 
            }

            m_buffer  = buffer;
            m_last    = count;
            m_current = offset;
        }

        internal NetworkOrderBinaryStream( byte[] buffer ) : this( buffer, 0, buffer.Length )
        {
        }

        //
        // Helper methods
        //

        internal void Advance( int count )
        {
            ThrowOnOutofBounds( count );

            m_current += count;
        }

        internal byte ReadByte()
        {
            ThrowOnOutofBounds( 1 );

            return m_buffer[ m_current++ ];
        }

        internal byte ReadByteNoAdvance()
        {
            return m_buffer[ m_current ];
        }

        internal ushort ReadUInt16()
        {
            ThrowOnOutofBounds( 2 );

            return (ushort)( ((uint)m_buffer[ m_current++ ] << 8) | 
                             ((uint)m_buffer[ m_current++ ]     ) );
        }

        internal uint ReadUInt32()
        {
            ThrowOnOutofBounds( 4 );

            return (    ((uint)m_buffer[ m_current++ ] << 24) |
                        ((uint)m_buffer[ m_current++ ] << 16) |
                        ((uint)m_buffer[ m_current++ ] <<  8) |
                        ((uint)m_buffer[ m_current++ ]      ) );
        }

        internal void ReadBytes( byte[] buffer, int offset, int count )
        {
            ThrowOnOutofBounds( count );

            System.Buffer.BlockCopy( m_buffer, m_current, buffer, offset, count );

            m_current += count; 
        }

        internal string ReadString( int count )
        {
            ThrowOnOutofBounds( 4 );
            
            var s = this.Encoding.GetString( m_buffer, m_current, count );

            m_current += count; 

            return s;
        }

        internal void WriteByte( byte b )
        {
            ThrowOnOutofBounds( 1 );

            m_buffer[ m_current++ ] = b;
        }

        internal void UpdateByteNoAdvance( byte b )
        {
            m_buffer[ m_current ] |= b;
        }

        internal void WriteUInt16( ushort v )
        {
            ThrowOnOutofBounds( 2 );

            m_buffer[ m_current++ ] = (byte)(v >> 8   );
            m_buffer[ m_current++ ] = (byte)(v &  0xFF); 
        }

        internal void WriteUInt32( uint v )
        {
            ThrowOnOutofBounds( 4 );
            
            m_buffer[ m_current++ ] = (byte)((v >> 24  ) & 0xFF);
            m_buffer[ m_current++ ] = (byte)((v >> 16  ) & 0xFF);
            m_buffer[ m_current++ ] = (byte)((v >>  8  ) & 0xFF);
            m_buffer[ m_current++ ] = (byte)((v        ) & 0xFF); 
        }

        internal void WriteBytes( byte[] buffer, int offset, int count )
        {
            ThrowOnOutofBounds( count );

            if(buffer.Length < offset + count)
            {
                throw new ArgumentException( ); 
            }

            System.Buffer.BlockCopy( buffer, offset, m_buffer, m_current, count );

            m_current += count; 
        }

        internal void WriteString( string s, Encoding encoding )
        {
            ThrowOnOutofBounds( 4 );

            if(s == null || encoding == null)
            {
                throw new ArgumentException( ); 
            }
            
            var bytes = encoding.GetBytes( s );
            
            WriteBytes( bytes, 0, bytes.Length ); 
        }

        internal long Available
        {
            get
            {
                return m_last - m_current;
            }
        }

        public override string ToString( )
        {
            var sb = new StringBuilder();

            for(int i = 0; i < m_buffer.Length; i++)
            {
                sb.Append( $"0x{m_buffer[ i ]:X}" ); 
                
                if(i < m_buffer.Length - 1)
                {
                    sb.Append( "," ); 
                }
            }

            return $"STREAM({sb})";
        }

        //
        // Access methods
        // 

        public byte[ ] Buffer
        {
            get
            {
                return m_buffer;
            }
        }

        public int Position
        {
            get
            {
                return m_current;
            }
        }

        public Encoding Encoding { get; internal set; }

        //--//
        
        private void ThrowOnOutofBounds( int requested )
        {
            if(requested > this.Available)
            {
                throw new IndexOutOfRangeException( ); 
            }
        }
    }
}
