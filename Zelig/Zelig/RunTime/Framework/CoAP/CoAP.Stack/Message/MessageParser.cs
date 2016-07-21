//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_PARSER

namespace CoAP.Stack
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Net;
    using CoAP.Common;

    public class MessageParser : IDisposable
    {
        //
        //   0                   1                   2                   3
        //   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |Ver| T |  TKL  |      Code     |            Message ID         |
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |                           Token (if any, TKL bytes) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |                           Options (if any) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //  |1 1 1 1 1 1 1 1|           Payload (if any) ...
        //  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //

        //
        // State 
        // 
        private static MessageParser s_parser;
        private        CoAPMessage   m_target;
        

        //--//

        //
        // Contructors
        //
        
        internal MessageParser( )
        {
        }

        public static MessageParser CheckOutParser( )
        {
            var parser = Interlocked.Exchange<MessageParser>( ref s_parser, null );

            if(parser == null)
            {
                return new MessageParser( ); 
            }

            return parser;
        }

        public static void CheckInParser( MessageParser parser )
        {
            Interlocked.Exchange<MessageParser>( ref s_parser, parser ); 
        }
        
        public void Dispose( )
        {
            CheckInParser( s_parser ); 
        }

        //
        // Helper methods
        //
        
        public bool Parse( CoAPMessage msg, IPEndPoint localEndPoint )
        {
            m_target = msg;

            bool fRes = Parse( m_target.Buffer, 0, m_target.Length );

            //m_target.Buffer = Constants.EmptyBuffer;

            msg.Context.Destination = MessageContext.ComputeDestination( msg.Options.Options, localEndPoint );

            return fRes;
        }

        public bool Inflate( CoAPMessage msg )
        {
            m_target = msg;

            bool fRes = Parse( m_target.Buffer, 0, m_target.Length );
            
            return fRes;
        }

        private bool Parse( byte[ ] buffer, int offset, int count )
        {
            //
            // We need at least the very first 4 bytes to get us an header
            //
            if(buffer.Length < Constants.MinimalMessageLength || count < Constants.MinimalMessageLength)
            {
                throw new ArgumentException( );
            }

            var stream = new NetworkOrderBinaryStream( buffer, offset, count );

            stream.Encoding = Common.Defaults.Encoding;

            CoAPMessage.Error error = CoAPMessageRaw.Error.None; 

            m_target.Header  = MatchHeader ( stream                       );
            m_target.Token   = MatchToken  ( stream, m_target.TokenLength );
            m_target.Options = MatchOptions( stream, ref error            );
            m_target.Payload = MatchPayload( stream                       );

            if(m_target.Context != null)
            {
                m_target.Context.Error = error;
            }
            
            return error == CoAPMessageRaw.Error.None;
        }

        //--//

        //
        // Access Methods 
        //

        private uint MatchHeader( NetworkOrderBinaryStream stream )
        {
            CheckAvailableOrThrow( stream, 4 ); 

            return stream.ReadUInt32( );
        }

        private MessageToken MatchToken( NetworkOrderBinaryStream stream, int length )
        {
            CheckAvailableOrThrow( stream, length );

            var bytes = new byte[ length ];

            stream.ReadBytes( bytes, 0, length ); 

            return new MessageToken( bytes ); 
        }

        private MessageOptions MatchOptions( NetworkOrderBinaryStream stream, ref CoAPMessage.Error error )
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

            var options = new MessageOptions();

            error = CoAPMessageRaw.Error.None;

            int delta         = 0;
            int previousDelta = 0;
            int  length;

            //
            // Loop until you find at least 1 byte, and it is not the payload marker
            //
            while(CheckAvailable( stream, 1 ))
            {
                var b = stream.ReadByte( );

                if(b == Constants.PayloadMarker)
                {
                    break;
                }

                delta  = (b & 0x0F); 
                length = (b & 0xF0) >> 4;
                
                delta  = CheckForDifferentialEncoding( stream, delta  ); 
                length = CheckForDifferentialEncoding( stream, length ); 

                var number = (MessageOption.OptionNumber)(previousDelta + delta);

                MessageOption opt = null;

                if(MessageOption.IsInteger( number ))
                {
                    CheckAvailableAndLengthOrThrow( stream, sizeof(uint), length ); 

                    var integerValue = stream.ReadUInt32(); 

                    opt = MessageOption_UInt.New( number, integerValue ); 

                    if(number == MessageOption.OptionNumber.Accept)
                    {
                        // If the preferred Content-Format cannot be returned, then 
                        // a 4.06 "Not Acceptable" MUST be sent as a response, unless 
                        // another error code takes precedence for this response.
                        //
                        if(integerValue != (uint)MessageOption.ContentFormat.Text_Plain__UTF8)
                        {
                            opt.IsNotAcceptable = true;
                            error               = CoAPMessageRaw.Error.Parsing__OptionError;
                        }
                    }
                }
                else if(MessageOption.IsString( number ))
                {
                    CheckAvailableAndLengthOrThrow( stream, 1, length );
                    
                    var stringValue = stream.ReadString( length ); 

                    opt = MessageOption_String.New( number, stringValue ); 
                }
                else /*if(MessageOption.IsByteArray( number ))*/
                {
                    //
                    // All other options, including unrecognized ones
                    // 

                    CheckAvailableAndLengthOrThrow( stream, 1, length );

                    var arrayValue = new byte[ length ]; 

                    stream.ReadBytes( arrayValue, 0, length ); 

                    opt = MessageOption_Opaque.New( number, arrayValue );

                    if(MessageOption.IsSupportedOption( number ) == false)
                    {
                        if(MessageOption.IsCriticalOption( (byte)number ) )
                        {
                            //
                            // Unrecognized options of class "critical" that occur in a Confirmable 
                            // request MUST cause the return of a 4.02( Bad Option ) response. 
                            // This response SHOULD include a diagnostic payload describing the 
                            // unrecognized option( s ) ( see Section 5.5.2 ).
                            // 
                            // Unrecognized options of class "critical" that occur in a Confirmable 
                            // response, or piggybacked in an Acknowledgement, MUST cause the response 
                            // to be rejected ( Section 4.2 ).
                            //
                            opt.IsBad  = true;
                            error      = CoAPMessageRaw.Error.Parsing__OptionError;
                        }
                        else
                        {
                            //
                            // Unrecognized options of class "elective" MUST be silently ignored.
                            //
                            opt.ShouldIgnore = true;
                        }
                    }
                }
                //else
                //{
                //    throw new CoAP_MessageFormatException( ); 
                //}

                opt.Delta = delta;
                
                //
                // Use Append, since options are in order.
                options.AppendToBack( opt );

                previousDelta = delta;
            }

            return options;
        }

        private MessagePayload MatchPayload( NetworkOrderBinaryStream stream )
        {
            //
            // Payload is anything remaining
            //
            var remaining = m_target.Length - stream.Position;

            var payload = new MessagePayload( new byte[ remaining ] );

            stream.ReadBytes( payload.Payload, 0, remaining );

            return payload;
        }

        private int CheckForDifferentialEncoding( NetworkOrderBinaryStream stream, int value )
        {
            if(value > 12)
            {
                if(value == 13)
                {
                    CheckAvailableOrThrow( stream, 1 );

                    var b = stream.ReadByte();

                    value += b;
                }
                else if(value == 14)
                {
                    value = 269;

                    CheckAvailableOrThrow( stream, 2 );

                    var bb = stream.ReadUInt16();

                    value += bb;
                }
                else
                {
                    throw new CoAP_MessageFormatException( );
                }
            }

            return value;
        }
        
        private static bool CheckAvailable( NetworkOrderBinaryStream stream, int count )
        {
            return stream.Available >= count;
        }

        [Conditional( "DEBUG_PARSER" )]
        private static void CheckAvailableOrThrow( NetworkOrderBinaryStream stream, int count )
        {
            if(CheckAvailable( stream, count ) == false)
            {
                throw new CoAP_MessageFormatException( ); 
            }
        }

        [Conditional( "DEBUG_PARSER" )]
        private static void CheckAvailableAndLengthOrThrow( NetworkOrderBinaryStream stream, int minimum, int advertized )
        {
            if(minimum > advertized || CheckAvailable( stream, advertized ) == false)
            {
                throw new CoAP_MessageFormatException( ); 
            }
        }
    }
}