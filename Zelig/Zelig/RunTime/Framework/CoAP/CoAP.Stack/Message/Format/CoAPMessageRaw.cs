//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System;
    using CoAP.Common;

    public class CoAPMessageRaw
    {
        //
        // CoAP Message structure
        //

        //
        //   0                   1                   2                   3
        //   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |Ver| T |  TKL  |      Code     |            Message ID         |  <== HEADER
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |                           Token (if any, TKL bytes) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |                           Options (if any) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |1 1 1 1 1 1 1 1|           Payload (if any) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //

        //
        // Code
        //

        //  0 1 2 3 4 5 6 7
        //  +-+-+-+-+-+-+-+-+
        //  |class| detail |
        //  +-+-+-+-+-+-+-+-+

        //  +------+------------------------------+-----------+
        //  | Code | Description                  | Reference |
        //  +------+------------------------------+-----------+
        //  | 2.01 | Created                      | [RFC7252] |
        //  | 2.02 | Deleted                      | [RFC7252] |
        //  | 2.03 | Valid                        | [RFC7252] |
        //  | 2.04 | Changed                      | [RFC7252] |
        //  | 2.05 | Content                      | [RFC7252] |
        //  | 4.00 | Bad Request                  | [RFC7252] |
        //  | 4.01 | Unauthorized                 | [RFC7252] |
        //  | 4.02 | Bad Option                   | [RFC7252] |
        //  | 4.03 | Forbidden                    | [RFC7252] |
        //  | 4.04 | Not Found                    | [RFC7252] |
        //  | 4.05 | Method Not Allowed           | [RFC7252] |
        //  | 4.06 | Not Acceptable               | [RFC7252] |
        //  | 4.12 | Precondition Failed          | [RFC7252] |
        //  | 4.13 | Request Entity Too Large     | [RFC7252] |
        //  | 4.15 | Unsupported Content-Format   | [RFC7252] |
        //  | 5.00 | Internal Server Error        | [RFC7252] |
        //  | 5.01 | Not Implemented              | [RFC7252] |
        //  | 5.02 | Bad Gateway                  | [RFC7252] |
        //  | 5.03 | Service Unavailable          | [RFC7252] |
        //  | 5.04 | Gateway Timeout              | [RFC7252] |
        //  | 5.05 | Proxying Not Supported       | [RFC7252] |
        //  +------+------------------------------+-----------+

        public enum ProtocolVersion : byte
        {
            Version_1 = 0x01,
        }

        public enum MessageType : byte
        {
            Confirmable     = 0x00,
            NonConfirmable  = 0x01,
            Acknowledgement = 0x02,
            Reset           = 0x03,
            Invalid         = 0x04,
        }

        public enum Class 
        {
            Request      = 0,
            Success      = 2,
            RequestError = 4,
            ServerError  = 5,
        }

        public enum Detail_Request : byte
        {
            Empty   = 0,
            GET     = 1,
            POST    = 2,
            PUT     = 3,
            DELETE  = 4,
        }

        public enum Detail_Success : byte
        {
            Created = 1,
            Deleted = 2,
            Valid   = 3,
            Changed = 4,
            Content = 5,
        }

        public enum Detail_RequestError : byte
        {
            BadRequest                  = 0,
            Unauthorized                = 1,
            BadOption                   = 2,
            Forbidden                   = 3,
            NotFound                    = 4,
            MethodNotAllowed            = 5,
            NotAcceptable               = 6,
            PreconditionFailed          = 12,
            RequestEntityTooLarge       = 13,
            UnsupportedContent_Format   = 15,
        }

        public enum Detail_ServerError : byte
        {
            InternalServerError     = 0,
            NotImplemented          = 1,
            BadGateway              = 2,
            ServiceUnavailable      = 3,
            GatewayTimeout          = 4,
            ProxyingNotSupported    = 5,
        }

        public enum Error
        {
            None                       ,
            Parsing__OptionError       ,
            Parsing__Malformed_NoHeader,
            Parsing__Malformed         ,
            Processing__AckNotReceived ,
        }

        //--//

        private const uint c_Version__Mask      = 0x00000003; // 2 bits
        private const  int c_Version__Shift     =          0;
        private const uint c_Type__Mask         = 0x0000000C; // 2 bits 
        private const  int c_Type__Shift        =          2;
        private const uint c_TokenLength__Mask  = 0x000000F0; // 4 bits 
        private const  int c_TokenLength__Shift =          4;
        private const uint c_Code__Mask         = 0x0000FF00; // 8 bits
        private const  int c_Code__Shift        =          8;
        private const uint c_Code_Detail__Mask  = 0x00001F00; // 5 bits
        private const  int c_Code_Detail__Shift =          8;
        private const uint c_Code_Class__Mask   = 0x0000E000; // 3 bits
        private const  int c_Code_Class__Shift  =         13;
        private const uint c_MessageId__Mask    = 0xFFFF0000; // 16 bits
        private const  int c_MessageId__Shift   =         16;

        //--//

        //
        // State 
        // 

        protected byte[ ]        m_buffer;
        protected uint           m_header;
        private MessageToken     m_token;
        protected MessageContext m_context;

        //--//

        //
        // Contructors
        //

        protected CoAPMessageRaw( ) : this( Constants.EmptyBuffer )
        {
        }

        protected CoAPMessageRaw( byte[ ] buffer )
        {
            this.Buffer = buffer;
            this.Token  = MessageToken.EmptyToken;
        }

        //--//

        internal void KillBuffer( )
        {
            m_buffer = null;
        }

        //--//

        public static CoAPMessageRaw NewBlankMessage( )
        {
            return new CoAPMessageRaw( );
        }

        //
        // Helper methods
        // 

        public bool IsAck
        {
            get
            {
                return (this.Type == MessageType.Acknowledgement); // ACK
            }
        }

        public bool IsEmptyAck
        {
            get
            {
                return (this.Type               == MessageType.Acknowledgement) && // ACK
                       (this.ClassCode          == Class.Request) && // is a request 
                       (this.DetailCode_Request == Detail_Request.Empty);   // is empty
            }
        }

        public bool IsPiggyBackedResponse
        {
            get
            {
                return (this.Type               == MessageType.Acknowledgement) && // ACK
                       (this.ClassCode          != Class.Request) && // not a request
                       (this.DetailCode_Request != Detail_Request.Empty);   // not empty
            }
        }

        public bool IsDelayedResponse
        {
            get
            {
                return ((this.Type               == MessageType.Confirmable)  ||
                        (this.Type               == MessageType.NonConfirmable)) && // CON or NON (not an ACK)
                        (this.ClassCode          != Class.Request) && // not a request
                        (this.DetailCode_Request != Detail_Request.Empty);   // not empty
            }
        }

        public bool IsConfirmable
        {
            get
            {
                return this.Type == MessageType.Confirmable; // CON 
            }
        }

        public bool IsReset
        {
            get
            {
                return this.Type == MessageType.Reset;
            }
        }

        public bool IsRequest
        {
            get
            {
                return this.ClassCode == Class.Request;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.IsRequest && this.DetailCode_Request == Detail_Request.Empty;
            }
        }

        public bool IsGET
        {
            get
            {
                return this.IsRequest && this.DetailCode_Request == Detail_Request.GET;
            }
        }

        public bool IsPOST
        {
            get
            {
                return this.IsRequest && this.DetailCode_Request == Detail_Request.POST;
            }
        }

        public bool IsPUT
        {
            get
            {
                return this.IsRequest && this.DetailCode_Request == Detail_Request.PUT;
            }
        }

        public bool IsDELETE
        {
            get
            {
                return this.IsRequest && this.DetailCode_Request == Detail_Request.DELETE;
            }
        }
        
        public bool IsNotGetorEmpty
        {
            get
            {
                return this.IsGET == false && this.IsEmpty == false;
            }
        }

        public bool IsPing
        {
            get
            {
                return (this.Type               == MessageType.Confirmable) && // CON  
                       (this.ClassCode          == Class.Request) && // is a request
                       (this.DetailCode_Request == Detail_Request.Empty);   // is empty;
            }
        }

        public override string ToString( )
        {
            return ToString( true );
        }

        protected string HeaderToString( )
        {
            return $"VERSION({this.Version})+TYPE({this.Type})+TKL({this.TokenLength})+CODE({(int)this.ClassCode}.{this.DetailCode:D2})+MESSAGEID({this.MessageId})";
        }

        protected string ToString( bool fOptionsAndPayload )
        {
            return $"HEADER({HeaderToString( )}),TOKEN({this.Token})" + (fOptionsAndPayload ? $",OPTIONS_AND_PAYLOAD({Utils.ByteArrayPrettyPrint(this.RawOptionsAndPayload)})" : String.Empty);
        }

        //
        // Access Methods 
        //

        public byte[ ] Buffer
        {
            get
            {
                return m_buffer;
            }
            set
            {
                m_header = ReadHeader( value );
                
                m_buffer = value;
            }
        }

        public MessageContext Context
        {
            get
            {
                return m_context;
            }
            set
            {
                m_context = value;
            }
        }

        public uint Header
        {
            get
            {
                return m_header;
            }
            set
            {
                m_header = value;
            }
        }

        public ProtocolVersion Version
        {
            get
            {
                unsafe
                {
                    return DecodeVersion( m_header );
                }
            }
        }

        public MessageType Type
        {
            get
            {
                unsafe
                {
                    return DecodeType( m_header );
                }
            }
        }

        public int TokenLength
        {
            get
            {
                unsafe
                {
                    return DecodeTokenLength( m_header );
                }
            }
        }

        public uint Code
        {
            get
            {
                unsafe
                {
                    return DecodeCode( m_header );
                }
            }
        }

        public Class ClassCode
        {
            get
            {
                unsafe
                {
                    return DecodeClass( m_header );
                }
            }
        }

        public byte DetailCode
        {
            get
            {
                unsafe
                {
                    return DecodeDetail( m_header );
                }
            }
        }

        public Detail_Request DetailCode_Request
        {
            get
            {
                unsafe
                {
                    return Request_DecodeDetail( m_header );
                }
            }
        }

        public Detail_Success DetailCode_Success
        {
            get
            {
                unsafe
                {
                    return Success_DecodeDetail( m_header );
                }
            }
        }

        public Detail_RequestError DetailCode_RequestError
        {
            get
            {
                unsafe
                {
                    return RequestError_DecodeDetail( m_header );
                }
            }
        }

        public Detail_ServerError DetailCode_ServerError
        {
            get
            {
                unsafe
                {
                    return ServerError_DecodeDetail( m_header );
                }
            }
        }

        public ushort MessageId
        {
            get
            {
                unsafe
                {
                    return (ushort)DecodeMessageId( m_header );
                }
            }
        }

        public int Length
        {
            get
            {
                return m_buffer.Length;
            }
        }

        public MessageToken Token
        {
            get
            {
                if(Object.ReferenceEquals( m_token, MessageToken.EmptyToken ))
                {
                    var tokenLength = this.TokenLength;

                    if(m_buffer.Length >= Constants.MinimalMessageLength + tokenLength)
                    {
                        var tokenBuffer = new byte[ this.TokenLength ];

                        Array.Copy( m_buffer, 4, tokenBuffer, 0, tokenLength );

                        m_token = new MessageToken( tokenBuffer );
                    }
                    else
                    {
                        throw new CoAP_MessageMalformedException( ); 
                    }
                }

                return m_token;
            }
            set
            {
                m_token = value;
            }
        }

        public byte[ ] RawOptionsAndPayload
        {
            get
            {
                var tokenBuffer = new byte[ 4 + this.TokenLength ];

                int offset = 4 + m_token.Size;
                int length = m_buffer.Length - offset;

                var buffer = new byte[ length ]; 

                Array.Copy( m_buffer, offset, buffer, 0, length );

                return buffer;
            }
        }

        //--//

        public static ProtocolVersion DecodeVersion( uint value )
        {
            return (ProtocolVersion)((value & c_Version__Mask) >> c_Version__Shift);
        }

        public static uint EncodeVersion( ProtocolVersion version )
        {
            return ((uint)version << c_Version__Shift) & c_Version__Mask;
        }

        public static MessageType DecodeType( uint value )
        {
            return (MessageType)((value & c_Type__Mask) >> c_Type__Shift);
        }

        public static uint EncodeType( MessageType type )
        {
            return ((uint)type << c_Type__Shift) & c_Type__Mask;
        }

        public static int DecodeTokenLength( uint value )
        {
            return (int)((value & c_TokenLength__Mask) >> c_TokenLength__Shift);
        }

        public static uint EncodeTokenLength( int length )
        {
            return ((uint)length << c_TokenLength__Shift) & c_TokenLength__Mask;
        }

        public static uint DecodeCode( uint value )
        {
            return ((value & c_Code__Mask) >> c_Code__Shift);
        }
        
        public static uint EncodeCode( byte value )
        {
            return ((uint)value << c_Code__Shift) & c_Code__Mask;
        }

        public static Class DecodeClass( uint value )
        {
            return (Class)((value & c_Code_Class__Mask) >> c_Code_Class__Shift);
        }

        public static uint EncodeClass( Class cls )
        {
            return ((uint)cls << c_Code_Class__Shift) & c_Code_Class__Mask;
        }

        public static byte DecodeDetail( uint value )
        {
            return (byte)((value & c_Code_Detail__Mask) >> c_Code_Detail__Shift);
        }

        public static uint EncodeDetail( byte details )
        {
            return ((uint)details << c_Code_Detail__Shift) & c_Code_Detail__Mask;
        }

        public static Detail_Request Request_DecodeDetail( uint value )
        {
            return (Detail_Request)((value & c_Code_Detail__Mask) >> c_Code_Detail__Shift);
        }

        public static uint Request_EncodeDetail( Detail_Request detail )
        {
            return EncodeClass( Class.Request ) | (((uint)detail << c_Code_Detail__Shift) & c_Code_Detail__Mask);
        }

        public static uint Request_WithDetail( Detail_Request detail )
        {
            return Request_EncodeDetail( detail ) >> 8;
        }

        public static Detail_Success Success_DecodeDetail( uint value )
        {
            return (Detail_Success)((value & c_Code_Detail__Mask) >> c_Code_Detail__Shift);
        }

        public static uint Success_EncodeDetail( Detail_Success detail )
        {
            return EncodeClass( Class.Success ) | (((uint)detail << c_Code_Detail__Shift) & c_Code_Detail__Mask);
        }

        public static uint Success_WithDetail( Detail_Success detail )
        {
            return Success_EncodeDetail( detail ) >> 8;
        }

        public static Detail_RequestError RequestError_DecodeDetail( uint value )
        {
            return (Detail_RequestError)((value & c_Code_Detail__Mask) >> c_Code_Detail__Shift);
        }

        public static uint RequestError_EncodeDetail( Detail_RequestError detail )
        {
            return EncodeClass( Class.RequestError ) | (((uint)detail << c_Code_Detail__Shift) & c_Code_Detail__Mask);
        }

        public static uint RequestError_WithDetail( Detail_RequestError detail )
        {
            return RequestError_EncodeDetail( detail ) >> 8;
        }

        public static Detail_ServerError ServerError_DecodeDetail( uint value )
        {
            return (Detail_ServerError)((value & c_Code_Detail__Mask) >> c_Code_Detail__Shift);
        }

        public static uint ServerError_EncodeDetail( Detail_ServerError detail )
        {
            return EncodeClass( Class.ServerError ) | (((uint)detail << c_Code_Detail__Shift) & c_Code_Detail__Mask);
        }

        public static uint ServerError_WithDetail( Detail_ServerError detail )
        {
            return ServerError_EncodeDetail( detail ) >> 8;
        }

        public static int DecodeMessageId( uint value )
        {
            return (ushort)((value & c_MessageId__Mask) >> c_MessageId__Shift);
        }

        public static uint EncodeMessageId( int id )
        {
            return ((uint)id << c_MessageId__Shift) & c_MessageId__Mask;
        }

        //--//

        private static uint ReadHeader( byte[ ] buffer )
        {
            if(buffer.Length >= Constants.MinimalMessageLength)
            {
                var header = ((uint)buffer[ 0 ] << 24) |
                             ((uint)buffer[ 1 ] << 16) |
                             ((uint)buffer[ 2 ] <<  8) |
                             ((uint)buffer[ 3 ])       ;
                
                return header;
            }

            return 0xFFFFFFFF;
        }

        private static uint ReadAndValidateHeader( byte[ ] buffer )
        {
            if(buffer.Length >= Constants.MinimalMessageLength)
            {
                return ValidateHeader( ReadHeader( buffer ) );
            }

            return 0xFFFFFFFF;
        }

        private static uint ValidateHeader( uint v )
        {
            ThrowIfNot( IsValidVersion    ( DecodeVersion    ( v )                    ) );
            ThrowIfNot( IsValidType       ( DecodeType       ( v )                    ) );
            ThrowIfNot( IsValidTokenLength( DecodeTokenLength( v )                    ) );
            ThrowIfNot( IsValidCode       ( DecodeClass      ( v ), DecodeDetail( v ) ) );

            return v;
        }

        private static bool IsValidVersion( ProtocolVersion version )
        {
            ThrowIfNot( version == ProtocolVersion.Version_1 );

            return true;
        }

        private static bool IsValidType( MessageType type )
        {
            ThrowIfNot( type <= MessageType.Invalid );

            return true;
        }

        private static bool IsValidTokenLength( int length )
        {
            ThrowIfNot( length <= 8 );

            return true;
        }

        private static bool IsValidCode( Class cls, byte detail )
        {
            switch(cls)
            {
                case Class.Request:
                case Class.Success:
                case Class.RequestError:
                case Class.ServerError:
                    {
                        if(detail >= 31)
                        {
                            ThrowIfNot( false );
                        }
                    }
                    break;

                default:
                    ThrowIfNot( false );
                    break;

            }

            return true;
        }

        private static void ThrowIfNot( bool condition )
        {
            if(condition == false)
            {
                throw new CoAP_MessageMalformedException( );
            }
        }
    }
}