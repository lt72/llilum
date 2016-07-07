//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using CoAP.Common;
    using CoAP.Stack.Abstractions;

    public class MessageBuilder : ICloneable
    {
        private class Unique : IUniqueRandom
        {
            private readonly Random m_random;

            internal Unique( int seed )
            {
                m_random = new Random( seed );
            }

            public byte[ ] GetBytes( int unique, byte[] bytes )
            {
                if(bytes.Length >= 1)
                {

                    m_random.NextBytes( bytes );

                    bytes[ 0 ] = (byte)unique;
                }

                return bytes;
            }

            public ushort GetShort( int unique )
            {
                return (ushort)((m_random.Next( ) & 0x00000FFF) | (unique & 0x0000F000)); 
            }
        }

        //--//
        //--//
        //--//

        private static readonly uint s_Header_Version__Clear     = 0xFFFFFFFC;
        private static readonly uint s_Header_Type__Clear        = 0xFFFFFFF3;
        private static readonly uint s_Header_TokenLength__Clear = 0xFFFFFF0F;
        //private static readonly uint s_Header_Code_Detail__Clear = 0xFFFFE0FF;
        //private static readonly uint s_Header_Code_Class__Clear  = 0xFFFF1FFF;
        private static readonly uint s_Header_Code_All__Clear    = 0xFFFF00FF;
        private static readonly uint s_Header_MessageId__Clear   = 0x0000FFFF;
        //--//
        private  static          IUniqueRandom  s_UniqueRandom   = new Unique( 0x75A55A5A    );
        private  static          int            s_DefaultUnique  = 0x0000A000;

        //
        // State 
        //

        private readonly IPEndPoint     m_destination;
        private readonly int            m_unique;
        private readonly IUniqueRandom  m_uniqueRandom;
        private volatile int            m_messageId;
        private          uint           m_header;
        private          MessageToken   m_token;
        private          MessageOptions m_options;
        private          MessageOptions m_persistentOptions;
        private          MessagePayload m_payload;

        //--//

        public static int DefaultUnique
        {
            get
            {
                return s_DefaultUnique;
            }
            set
            {
                s_DefaultUnique = value;
            }
        }

        public static IUniqueRandom UniqueRandom
        {
            get
            {
                return s_UniqueRandom;
            }
            set
            {
                s_UniqueRandom = value;
            }
        }

        //
        // Contructors
        //

        private MessageBuilder( IPEndPoint destination, int unique, IUniqueRandom uniqueRandom )
        {
            m_destination  = destination;
            m_unique       = unique;
            m_uniqueRandom = uniqueRandom;
            m_header       = 0;
            m_token        = MessageToken.EmptyToken;
            m_options      = new MessageOptions( );
            m_payload      = MessagePayload.EmptyPayload;
        }

        private MessageBuilder( IPEndPoint destination, int unique ) : this( destination, unique, MessageBuilder.UniqueRandom )
        {
        }

        private MessageBuilder( IPEndPoint destination ) : this( destination, MessageBuilder.DefaultUnique, MessageBuilder.UniqueRandom )
        {
        }

        //--//

        public static MessageBuilder Create( IPEndPoint intermediary, ServerCoAPUri uri )
        {
            if(intermediary == null)
            {
                return Create( uri ); 
            }

            string scheme = null, host = null, path = null;
            int port = 0;
            var options = new MessageOptions();

            if(uri != null)
            {
                bool fSecure = CoAPUri.UriToComponents( uri.ToString(), intermediary, out scheme, out host, out port, out path, options );
            }

            return new MessageBuilder( intermediary ).WithMessageId( ).WithPersistentOptions( options );
        }

        public static MessageBuilder Create( ServerCoAPUri uri )
        {
            return new MessageBuilder( uri?.EndPoints[ 0 ] ).WithMessageId( );
        }

        public static MessageBuilder Create( IPEndPoint destination )
        {
            return new MessageBuilder( destination ).WithMessageId( );
        }
        
        public object Clone( )
        {
            return new MessageBuilder( this.Destination, this.m_unique, this.m_uniqueRandom )
                .WithMessageId        ( m_messageId )
                .WithPersistentOptions( m_persistentOptions )
                .WithOptions          ( m_options );
        }

        //
        // Helper methods
        //

        public MessageBuilder CreateEmptyRequest( )
        {
            return this
                .WithVersion( CoAPMessage.ProtocolVersion.Version_1 )
                .WithType( CoAPMessage.MessageType.Confirmable )
                .WithTokenLength( 0 )
                .WithRequestCode( CoAPMessage.Detail_Request.Empty )
                .WithMessageId( );
        }

        public MessageBuilder CreateResponse( CoAPMessageRaw request )
        {
            return this
                .WithVersion( request.Version )
                .WithTokenLength( request.TokenLength )
                .WithToken( request.Token );
        }

        public MessageBuilder CreateAck( CoAPMessage response, MessageContext messageCtx )
        {
            var request = messageCtx.Message;

            return CreateResponse( request )
                .WithType( CoAPMessage.MessageType.Acknowledgement )
                .WithMessageId( response.MessageId );
        }

        public MessageBuilder CreateAck( MessageContext messageCtx )
        {
            var request = messageCtx.Message;

            return CreateResponse( request )
                .WithType( CoAPMessage.MessageType.Acknowledgement )
                .WithMessageId( request.MessageId );
        }

        public MessageBuilder CreateDelayedResponse( MessageContext messageCtx )
        {
            var request = messageCtx.Message;

            return CreateResponse( request )
                .WithType( request.Type )
                .WithCode( messageCtx.ResponseCode )
                .WithMessageId( );
        }

        public MessageBuilder CreateResetResponse( MessageContext messageCtx )
        {
            var request = messageCtx.Message;

            return CreateResponse( request )
                .WithType( CoAPMessage.MessageType.Reset )
                .WithMessageId( request.MessageId )
                .WithRequestCode( CoAPMessage.Detail_Request.Empty );
        }

        public MessageBuilder FromContext( MessageContext messageCtx )
        {
            var msg = messageCtx.MessageInflated;

            return this
                .WithHeader ( msg.Header )
                .WithToken  ( msg.Token )
                .WithOptions( msg.Options )
                .WithPayload( msg.Payload );
        }

        public MessageBuilder WithHeader( uint rawHeader )
        {
            m_header = rawHeader;

            return this;
        }

        public MessageBuilder WithVersion( CoAPMessage.ProtocolVersion version )
        {
            m_header &= s_Header_Version__Clear;
            m_header |= CoAPMessage.EncodeVersion( version );

            return this;
        }

        public MessageBuilder WithType( CoAPMessage.MessageType type )
        {
            m_header &= s_Header_Type__Clear;
            m_header |= CoAPMessage.EncodeType( type );

            return this;
        }

        public MessageBuilder WithTokenLength( int length )
        {
            m_header &= s_Header_TokenLength__Clear;
            m_header |= CoAPMessage.EncodeTokenLength( length );

            if(length > 0)
            {
                m_token = new MessageToken( length, this ); 
            }

            return this;
        }

        public MessageBuilder WithCode( uint code )
        {
            Debug.Assert( code <= 0xFF ); 

            m_header &= s_Header_Code_All__Clear;
            m_header |= CoAPMessage.EncodeCode( (byte)code );

            return this;
        }

        public MessageBuilder WithRequestCode( CoAPMessage.Detail_Request detail )
        {
            m_header &= s_Header_Code_All__Clear;
            m_header |= CoAPMessage.EncodeClass( CoAPMessage.Class.Request );
            m_header |= CoAPMessage.Request_EncodeDetail( detail );

            return this;
        }

        public MessageBuilder WithSuccessCode( CoAPMessage.Detail_Success detail )
        {
            m_header &= s_Header_Code_All__Clear;
            m_header |= CoAPMessage.EncodeClass( CoAPMessage.Class.Success );
            m_header |= CoAPMessage.Success_EncodeDetail( detail );

            return this;
        }

        public MessageBuilder WithRequestErrorCode( CoAPMessage.Detail_RequestError detail )
        {
            m_header &= s_Header_Code_All__Clear;
            m_header |= CoAPMessage.EncodeClass( CoAPMessage.Class.RequestError );
            m_header |= CoAPMessage.RequestError_EncodeDetail( detail );

            return this;
        }

        public MessageBuilder WithServerErrorCode( CoAPMessage.Detail_ServerError detail )
        {
            m_header &= s_Header_Code_All__Clear;
            m_header |= CoAPMessage.EncodeClass( CoAPMessage.Class.ServerError );
            m_header |= CoAPMessage.ServerError_EncodeDetail( detail );

            return this;
        }

        public MessageBuilder WithMessageId( )
        {
            WithMessageId( NewMessageId( ) ); 

            return this;
        }

        public MessageBuilder WithMessageId( int id )
        {
            m_messageId = id & 0x0000FFFF;

            return this;
        }

        private MessageBuilder WithToken( MessageToken token )
        {
            if(token == null)
            {
                token = MessageToken.EmptyToken;
            }

            m_token = token;

            return this;
        }

        public MessageBuilder WithOption( MessageOption option )
        {
            m_options.InsertInOrder( option );

            return this;
        }

        public MessageBuilder WithOptions( MessageOptions options )
        {
            if(options == null)
            {
                options = MessageOptions.EmptyOptions;
            }

            m_options = options;

            return this;
        }

        public MessageBuilder WithPersistentOptions( MessageOptions options )
        {
            m_persistentOptions = options;

            return this;
        }

        public MessageBuilder WithPayload( byte[ ] payload )
        {
            m_payload = new MessagePayload( payload );

            return this;
        }

        public MessageBuilder WithPayload( MessagePayload payload )
        {
            if(payload == null)
            {
                payload = MessagePayload.EmptyPayload;
            }

            m_payload = payload;

            return this;
        }

        public CoAPMessageRaw Build( )
        {
            return BuildInternal( false );
        }

        public CoAPMessageRaw BuildAndReset( )
        {
            return BuildInternal( true ); 
        }

        private CoAPMessageRaw BuildInternal( bool fReset )
        {
            var msg = CoAPMessageRaw.NewBlankMessage( );

            if(m_persistentOptions != null)
            {
                foreach(var option in m_persistentOptions.Options)
                {
                    m_options.InsertInOrder( option );
                }
            }

            //
            // encode message ID as it was selected last and then bump it up
            //
            int id1 = -1;
            int id2 = -1;

            do
            {
                id1 = m_messageId;

                id2 = Interlocked.CompareExchange( ref m_messageId, id1 + 1, id1 );
            } while(id1 != id2);

            m_header &= s_Header_MessageId__Clear;
            m_header |= CoAPMessage.EncodeMessageId( id2 );
            
            //
            // Set up buffer
            //

            EnsureBuffer( msg );

            //
            // Complete the message
            //

            var stream = new NetworkOrderBinaryStream( msg.Buffer );

            stream.WriteUInt32( m_header );

            m_token  .Encode( stream );
            m_options.Encode( stream );

            if(m_payload != null)
            {
                m_payload.Encode( stream );
            }

            if(fReset)
            {
                Reset( );
            }

            return msg;
        }
        
        public MessageBuilder Reset()
        {
            m_header  = 0;
            m_token   = MessageToken.EmptyToken;
            m_payload = MessagePayload.EmptyPayload;

            m_options.Reset( ); 

            return this;
        }

#if DESKTOP
        public override string ToString( )
        {
            return $"MESSAGE[HEADER({HeaderToString( )})](TOKEN({m_token}),OPTIONS({m_options}),PAYLOAD({m_payload}))";
        }
#endif

        //
        // Access methods
        //

        public IPEndPoint Destination
        {
            get
            {
                var destination = MessageContext.ComputeDestination( m_options.Options, m_destination ); 

                if(destination != null && destination.Equals( m_destination ) == false)
                {
                    return destination;
                }

                return m_destination;
            }
        }

        //--//

        public byte[ ] NewToken( byte[ ] bytes )
        {
            return m_uniqueRandom.GetBytes( m_unique, bytes );
        }

        //--//

        private void EnsureBuffer( CoAPMessageRaw msg )
        {
            if(Object.ReferenceEquals( msg.Buffer, Constants.EmptyBuffer)) 
            {
                msg.Buffer = new byte[ ComputeSize( ) ];
            }
        }

        private string HeaderToString( )
        {
            return $"[VERSION({CoAPMessage.DecodeVersion(m_header)})+TYPE({CoAPMessage.DecodeType( m_header )})+TKL({CoAPMessage.DecodeTokenLength( m_header )})+CODE({(int)CoAPMessage.DecodeClass( m_header )}.{(int)CoAPMessage.DecodeDetail( m_header ):D2})+MESSAGEID({m_messageId})]";
        }

        private int ComputeSize( )
        {
            m_options.UpdateDeltas( );

            int size = 4 + 
                m_token  .Size +
                m_options.Size + 
                m_payload.Size ;

            return size;
        }

        private int NewMessageId()
        {
            return m_uniqueRandom.GetShort( m_unique ); 
        }
    }
}
