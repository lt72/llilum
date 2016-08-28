//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System;
    using CoAP.Common;
    using System.Text;

    public abstract class MessageOption : IEncodable
    {
        // +--------------------------+----------+----+------------------------+
        // | Media type               | Encoding | ID | Reference              |
        // +--------------------------+----------+----+------------------------+
        // | text/plain;              |     -    |  0 | [RFC2046] [RFC3676]    |
        // | charset=utf-8            |          |    | [RFC5147]              |
        // | application/link-format  |     -    | 40 | [RFC6690]              |
        // | application/xml          |     -    | 41 | [RFC3023]              |
        // | application/octet-stream |     -    | 42 | [RFC2045] [RFC2046]    |
        // | application/exi          |     -    | 47 | [REC-exi-20140211]     |
        // | application/json         |     -    | 50 | [RFC7159]              |
        // +--------------------------+----------+----+------------------------+

        public enum ContentFormat
        {
            Text_Plain__UTF8          = 0,
            Application__Link_Format  = 40,
            Application__Xml          = 41,
            Application__octet_Stream = 42,
            Application__Exi          = 47,
            Application__Json         = 50,
        }

        public enum OptionNumber : byte
        {
            If_Match        = 1,   
            Uri_Host        = 3,   
            ETag            = 4, 
            If_None_Match   = 5,
            Uri_Port        = 7, 
            Location_Path   = 8, 
            Uri_Path        = 11, 
            Content_Format  = 12, 
            Max_Age         = 14, 
            Uri_Query       = 15, 
            Accept          = 17,
            Location_Query  = 20, 
            Proxy_Uri       = 35, 
            Proxy_Scheme    = 39, 
            Size1           = 60,
            Unknown         = 255,
        }
        
        internal struct OptionEntry
        {
            internal byte   Number;
            internal bool   IsCritical;
            internal bool   IsUnsafe;
            internal bool   NoCacheKey;
            internal bool   IsRepeatable;
            internal int    NameLength;
            internal string Name;
            internal object Default;

            //--//

            internal OptionEntry( byte number, bool critical, bool notsafe, bool noCachekey, bool repeatable, int length, string name, object defaultValue )
            {
                this.Number       = number; 
                this.IsCritical   = critical;
                this.IsUnsafe     = notsafe;
                this.NoCacheKey   = noCachekey;
                this.IsRepeatable = repeatable;
                this.NameLength   = length; 
                this.Name         = name;
                this.Default      = defaultValue;
            }
        }

        //--//

        private const byte c_Delta__13  = 0x0D;
        private const byte c_Delta__14  = 0x0E;
        private const byte c_Length__13 = 0xD0;
        private const byte c_Length__14 = 0xE0;

        //--//

        private static readonly int           SupportedOptions = 15;
        private static readonly OptionEntry[] OptionMap;

        //--//

        //
        // State
        //

        private readonly OptionNumber m_number;
        private readonly byte[]       m_bytes;
        private          bool         m_badOption;
        private          bool         m_notAcceptable;
        private          bool         m_ignoreOption;

        //--//

        //  +-----+---+---+---+---+----------------+--------+--------+----------+
        //  | No. | C | U | N | R | Name           | Format | Length | Default  |
        //  +-----+---+---+---+---+----------------+--------+--------+----------+
        //  |   1 | x |   |   | x | If-Match       | opaque | 0-8    | (none)   |
        //  |   3 | x | x | - |   | Uri-Host       | string | 1-255  | (*)      |
        //  |   4 |   |   |   | x | ETag           | opaque | 1-8    | (none)   |
        //  |   5 | x |   |   |   | If-None-Match  | empty  | 0      | (none)   |
        //  |   7 | x | x | - |   | Uri-Port       | uint   | 0-2    | (*)      |
        //  |   8 |   |   |   | x | Location-Path  | string | 0-255  | (none)   |
        //  |  11 | x | x | - | x | Uri-Path       | string | 0-255  | (none)   |
        //  |  12 |   |   |   |   | Content-Format | uint   | 0-2    | (none)   |
        //  |  14 |   | x | - |   | Max-Age        | uint   | 0-4    | 60       |
        //  |  15 | x | x | - | x | Uri-Query      | string | 0-255  | (none)   |
        //  |  17 | x |   |   |   | Accept         | uint   | 0-2    | (none)   |
        //  |  20 |   |   |   | x | Location-Query | string | 0-255  | (none)   |
        //  |  35 | x | x | - |   | Proxy-Uri      | string | 1-1034 | (none)   |
        //  |  39 | x | x | - |   | Proxy-Scheme   | string | 1-255  | (none)   |
        //  |  60 |   |   | x |   | Size1          | uint   | 0-4    | (none)   |
        //  +-----+---+---+---+---+----------------+--------+--------+----------+

        //--//

        //
        // Contructors
        //

        static MessageOption( )
        {
            OptionMap = new OptionEntry[ SupportedOptions + 1 ];
            
            OptionMap[ OptionNumberToIndex( OptionNumber.If_Match       ) ] = new OptionEntry(   1, true , false, false, true , 10, "If-Match"      , null ); 
            OptionMap[ OptionNumberToIndex( OptionNumber.Uri_Host       ) ] = new OptionEntry(   3, true , true , false, false, 10, "Uri-Host"      , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.ETag           ) ] = new OptionEntry(   4, false, false, false, true , 10, "ETag"          , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.If_None_Match  ) ] = new OptionEntry(   5, true , false, false, false, 15, "If-None-Match" , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Uri_Port       ) ] = new OptionEntry(   7, true , true , false, false, 10, "Uri-Port"      , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Location_Path  ) ] = new OptionEntry(   8, false, false, false, true , 15, "Location-Path" , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Uri_Path       ) ] = new OptionEntry(  11, true , true , false, true , 10, "Uri-Path"      , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Content_Format ) ] = new OptionEntry(  12, false, false, false, false, 16, "Content-Format", null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Max_Age        ) ] = new OptionEntry(  14, false, true , false, false,  9, "Max-Age"       , 60   );
            OptionMap[ OptionNumberToIndex( OptionNumber.Uri_Query      ) ] = new OptionEntry(  15, true , true , false, true , 11, "Uri-Query"     , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Accept         ) ] = new OptionEntry(  17, true , false, false, false,  8, "Accept"        , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Location_Query ) ] = new OptionEntry(  20, false, false, false, true , 16, "Location-Query", null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Proxy_Uri      ) ] = new OptionEntry(  35, true , true , false, false, 11, "Proxy-Uri"     , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Proxy_Scheme   ) ] = new OptionEntry(  39, true , true , false, false, 14, "Proxy-Scheme"  , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Size1          ) ] = new OptionEntry(  60, false, false, true , false,  7, "Size1"         , null );
            OptionMap[ OptionNumberToIndex( OptionNumber.Unknown        ) ] = new OptionEntry( 255, false, false, false, false,  5, "UNK"           , null );
        }
        
        internal MessageOption( OptionNumber option, byte[ ] value )
        {
            m_number = option;
            m_bytes  = value;
        }

        //
        // Helper Methods
        // 

        public override bool Equals( object obj )
        {
            if(obj == null)
            {
                return false;
            }

            MessageOption option = obj as MessageOption;

            if(option == null)
            {
                return false;
            }

            return this.m_number == option.m_number && Utils.ByteArrayCompare( this.m_bytes, option.m_bytes );
        }

        public override int GetHashCode( )
        {
            return (int)((int)m_number ^ Utils.ByteArrayToHash( m_bytes ));
        }

        public static bool operator ==( MessageOption optA, MessageOption optB )
        {
            if((object)optA == null && (object)optB == null)
            {
                return true;
            }

            if((object)optA != null && (object)optB != null)
            {
                return optA.Equals( optB );
            }

            return false;
        }
        public static bool operator !=( MessageOption optA, MessageOption optB )
        {
            return !(optA == optB);
        }


        public virtual void Encode( NetworkOrderBinaryStream stream )
        {
            //    0   1   2   3    4   5   6   7
            //  +---------------+---------------+
            //  |               |               |
            //  | Option Delta  | Option Length | 1 byte
            //  |               |               |
            //  +---------------+---------------+
            //  \                               \
            //  /         Option Delta          / 0-2 bytes
            //  \          (extended)           \
            //  +-------------------------------+
            //  \                               \
            //  /        Option Length          / 0-2 bytes
            //  \          (extended)           \
            //  +-------------------------------+
            //  \                               \
            //  /                               /
            //  \                               \
            //  /       Option Value            / 0 or more bytes
            //  \                               \
            //  /                               /
            //  \                               \
            //  +-------------------------------+

            var delta  = this.Delta;
            var length = this.ValueLength;

            var delta1  = delta  - 13;
            var length1 = length - 13;

            if(delta <= 12)
            {
                //
                // Standard encoding, 4 bits into the lower part of the first byte
                // 
                stream.UpdateByteNoAdvance( (byte)(delta & 0x0000000F) );
            }
            else
            {
                //
                // Need to use remainder, set the markers
                //
                if(delta1 <= 255)
                {
                    stream.UpdateByteNoAdvance( (byte)(c_Delta__13) );
                }
                else
                {
                    delta1 -= 256;
                    stream.UpdateByteNoAdvance( (byte)(c_Delta__14) );
                }
            }

            if(length <= 12)
            {
                //
                // Standard encoding, 4 bits into the upper part of the first byte
                // 
                stream.UpdateByteNoAdvance( (byte)((length & 0x0000000F) << 4) );
            }
            else
            {
                //
                // Need to use remainder, set the markers
                //
                if(length1 <= 255)
                {
                    stream.UpdateByteNoAdvance( (byte)(c_Length__13) );
                }
                else
                {
                    length1 -= 256;
                    stream.UpdateByteNoAdvance( (byte)(c_Length__14) );
                }
            }

            stream.Advance( 1 );

            //
            // Complete option delta remainder
            // 
            if(delta > 12)
            {
                //
                // we need one or two additional bytes
                //

                if(delta1 <= 255)
                {
                    stream.WriteByte( (byte)(delta1 & 0x000000FF) );
                }
                else
                {
                    delta1 -= 256;
                    stream.WriteByte( (byte)((delta1 & 0x0000FF00) >> 8) );
                    stream.WriteByte( (byte)((delta1 & 0x000000FF)     ) );
                }
            }

            //
            // Complete option length remainder
            // 

            if(length > 12)
            {
                //
                // we need one or two additional bytes
                //

                if(length1 <= 255)
                {
                    stream.WriteByte( (byte)(length1 & 0x000000FF) );
                }
                else
                {
                    length1 -= 256;
                    stream.WriteByte( (byte)((length1 & 0x0000FF00) >> 8) );
                    stream.WriteByte( (byte)((length1 & 0x000000FF)     ) );
                }
            }
        }
        
        //
        // Access Methods
        //

        public byte[ ] RawBytes
        {
            get
            {
                return m_bytes;
            }
        }

        public virtual object Value
        {
            get
            {
                return this.RawBytes;
            }
        }

        public virtual int ValueLength
        {
            get
            {
                return m_bytes.Length;
            }
        }

        internal int Delta
        {
            get; set;
        }

        public OptionNumber Number
        {
            get
            {
                return m_number;
            }
        }

        public bool IsCritical
        {
            get
            {
                return IsCriticalOption( (byte)m_number ); 
                //return OptionMap[ OptionNumberToIndex( m_option ) ].IsCritical;
            }
        }

        public bool IsSafeToForward
        {
            get
            {
                return IsSafeOption( (byte)m_number );
                //return OptionMap[ OptionNumberToIndex( m_option ) ].IsUnsafe;
            }
        }

        public bool NoCacheKey
        {
            get
            {
                return IsNoCacheKeyOption( (byte)m_number );
                //return OptionMap[ OptionNumberToIndex( m_option ) ].NoCacheKey;
            }
        }

        public bool IsRepeatable
        {
            get
            {
                if(IsSupportedOption( m_number ))
                {
                    return OptionMap[ OptionNumberToIndex( m_number ) ].IsRepeatable;
                }

                return OptionMap[ OptionNumberToIndex( OptionNumber.Unknown ) ].IsRepeatable;
            }
        }

        public string Name
        {
            get
            {
                if(IsSupportedOption( m_number ))
                {
                    return OptionMap[ OptionNumberToIndex( m_number ) ].Name;
                }
                
                return OptionMap[ OptionNumberToIndex( OptionNumber.Unknown ) ].Name;
            }
        }

        public int NameLength
        {
            get
            {
                if(IsSupportedOption( m_number ))
                {
                    return OptionMap[ OptionNumberToIndex( m_number ) ].NameLength;
                }

                return OptionMap [OptionNumberToIndex( OptionNumber.Unknown )].NameLength;
            }
                
        }

        public object Default
        {
            get
            {
                if(IsSupportedOption( m_number ))
                {
                    return OptionMap[ OptionNumberToIndex( m_number ) ].Default;
                }

                return OptionMap[ OptionNumberToIndex( OptionNumber.Unknown ) ].Default;
            }
        }

        public bool IsUriPath
        {
            get
            {
                if(this.Number == OptionNumber.Uri_Path)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsUriQuery
        {
            get
            {
                if(this.Number == OptionNumber.Uri_Query)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsMaxAge
        {
            get
            {
                if(this.Number == OptionNumber.Max_Age)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsHostUri
        {
            get
            {
                if(this.Number == OptionNumber.Uri_Host)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsProxyUri
        {
            get
            {
                if(this.Number == OptionNumber.Proxy_Uri)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsUri
        {
            get
            {
                return IsHostUri || IsProxyUri;
            }
        }

        public bool IsPort
        {
            get
            {
                if(this.Number == OptionNumber.Uri_Port)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsETag
        {
            get
            {
                if(this.Number == OptionNumber.ETag)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsBad
        {
            get
            {
                return m_badOption;
            }
            internal set
            {
                m_badOption = value;
            }
        }

        public bool IsNotAcceptable
        {
            get
            {
                return m_notAcceptable;
            }
            internal set
            {
                m_notAcceptable = value;
            }
        }

        public bool ShouldIgnore
        {
            get
            {
                return m_ignoreOption;
            }
            internal set
            {
                m_ignoreOption = value;
            }
        }

        public static bool IsString( OptionNumber number )
        {
            switch(number)
            {
                case OptionNumber.Uri_Host      :
                case OptionNumber.Location_Path :
                case OptionNumber.Uri_Path      :
                case OptionNumber.Uri_Query     :
                case OptionNumber.Location_Query:
                case OptionNumber.Proxy_Uri     :
                case OptionNumber.Proxy_Scheme  :
                    return true;
                    
                default:
                    return false; 
            }
        }        
        
        public static bool IsByteArray( OptionNumber number )
        {
            switch(number)
            {
                case OptionNumber.If_Match     :
                case OptionNumber.If_None_Match:
                case OptionNumber.ETag         :
                    return true;
                    
                default:
                    return false; 
            }
        }        
        
        public static bool IsInteger( OptionNumber number )
        {
            switch(number)
            {
                case OptionNumber.Uri_Port      :
                case OptionNumber.Content_Format:
                case OptionNumber.Max_Age       :
                case OptionNumber.Accept        :
                case OptionNumber.Size1         :
                    return true;
                    
                default:
                    return false; 
            }
        }

        public static bool IsSupportedOption( OptionNumber number )
        {
            return OptionNumberToIndex( number ) < SupportedOptions; 
        }

        public static bool IsCriticalOption( byte number )
        {
            return (number & 0x01) == 0x01;
        }

        public static bool IsSafeOption( byte number )
        {
            return !((number & 0x02) == 0x02);
        }

        public static bool IsNoCacheKeyOption( byte number )
        {
            return (number & 0x1e) != 0x1c; 
        }

        //--//

        private static int OptionNumberToIndex( OptionNumber number )
        {
            switch(number)
            {
                case OptionNumber.If_Match:
                    return 0;
                case OptionNumber.Uri_Host:
                    return 1;
                case OptionNumber.ETag:
                    return 2;
                case OptionNumber.If_None_Match:
                    return 3;
                case OptionNumber.Uri_Port:
                    return 4;
                case OptionNumber.Location_Path:
                    return 5;
                case OptionNumber.Uri_Path:
                    return 6;
                case OptionNumber.Content_Format:
                    return 7;
                case OptionNumber.Max_Age:
                    return 8;
                case OptionNumber.Uri_Query:
                    return 9;
                case OptionNumber.Accept:
                    return 10;
                case OptionNumber.Location_Query:
                    return 11;
                case OptionNumber.Proxy_Uri:
                    return 12;
                case OptionNumber.Proxy_Scheme:
                    return 13;
                case OptionNumber.Size1:
                    return 14;
                default:
                    return SupportedOptions; 
            } 
        }
    }
}

