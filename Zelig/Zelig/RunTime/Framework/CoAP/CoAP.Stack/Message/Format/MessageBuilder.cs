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
        private class UniqueRandom : IUniqueRandom
        {
            private readonly Random m_random;

            internal UniqueRandom( int seed )
            {
                m_random = new Random( seed );
            }

            public byte[ ] GetBytes( byte[] bytes )
            {
                if(bytes.Length > 0)
                {
                    m_random.NextBytes( bytes );
                }

                return bytes;
            }

            public ushort GetShort( )
            {
                //
                // use uniqueness on 4th byte
                //
                return (ushort)m_random.Next( );
            }

            public int GetInt( )
            {
                //
                // use uniqueness on lower 4 bytes
                //
                return m_random.Next( );
            }
        }

        private enum NoOptions
        {
            All     , 
            JustETag,
            None    ,
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

        public static readonly IUniqueRandom RandomNumberGenerator = new UniqueRandom( 0x75A55A5A );

        //--//
        
            //
        // State 
        //

        private readonly IPEndPoint     m_destination;
        private readonly IUniqueRandom  m_uniqueRandom;
        private volatile int            m_messageId;

        private          uint           m_header;
        private          MessageToken   m_token;
        private          MessageOptions m_options;
        private          MessageOptions m_persistentOptions;
        private          MessagePayload m_payload;
        private          MessageContext m_context;
        private          NoOptions      m_doNotUseOptions;

        //--//

        //
        // Contructors
        //
        
        private MessageBuilder( IPEndPoint destination )
        {
            m_destination       = destination;
            m_uniqueRandom      = new UniqueRandom( RandomNumberGenerator.GetInt( ) );
            m_header            = 0;
            m_token             = MessageToken.EmptyToken;
            m_options           = new MessageOptions( );
            m_persistentOptions = new MessageOptions( );
            m_payload           = MessagePayload.EmptyPayload;
            m_doNotUseOptions   = NoOptions.All;
        }

        //--//

        public static MessageBuilder Create( IPEndPoint intermediary, CoAPServerUri uri )
        {
            //string scheme = null, host = null, path = null;
            //int port = 0;
            MessageOptions options = MessageOptions.EmptyOptions;

            if(uri != null)
            {
                options = (MessageOptions)uri.Options.Clone( );

                if(intermediary == null)
                {
                    intermediary = uri.EndPoints[ 0 ];
                }

                //if(uri != null)
                //{
                //    CoAPUri.UriStringToComponents( uri.ToString(), ref intermediary, out scheme, out host, out port, out path, options );
                //}

                //
                // To create the options to support proxying we need to check the uri 
                // for any endpoints that are not equivalent to the intermediary. 
                //
                foreach(var ep in uri.EndPoints)
                {
                    if(ep.Equals( intermediary ) == false)
                    {
                        var host = new MessageOption_String( MessageOption.OptionNumber.Uri_Host, ep.Address.ToString());
                        var port = new MessageOption_Int  ( MessageOption.OptionNumber.Uri_Port, ep.Port               );

                        if(options.Contains( host ) == false)
                        {
                            options.Add( host );
                        }
                        if(options.Contains( port ) == false)
                        {
                            options.Add( port );
                        }
                    }
                }
            }

            return new MessageBuilder( intermediary ).WithMessageId( ).WithPersistentOptions( options );
        }

        public static MessageBuilder Create( IPEndPoint destination )
        {
            return new MessageBuilder( destination ).WithMessageId( );
        }
        
        public object Clone( )
        {
            return new MessageBuilder( this.Destination )
                .WithMessageId        ( m_messageId         )
                .WithPersistentOptions( m_persistentOptions )
                .WithOptions          ( m_options           );
        }

        //
        // Helper methods
        //

        //
        // Simple messages, no options: EMPTY request (CoAP ping), ACK, RESET
        // 

        public MessageBuilder CreateEmptyRequest( )
        {
            return this
                .WithNoOptions  ( )
                .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                .WithTokenLength( 0 )
                .WithType       ( CoAPMessage.MessageType.Confirmable )
                .WithRequestCode( CoAPMessage.Detail_Request.Empty )
                .WithMessageId  ( );
        }

        //
        // All responses
        // 

        private MessageBuilder CreateResponse( CoAPMessageRaw request, MessageContext ctx )
        {
            var response = this
                .WithVersion    ( request.Version     )
                .WithTokenLength( request.TokenLength )
                .WithToken      ( request.Token       )
                .WithContext    ( ctx                 );

            return response;
        }

        //
        // Simple messages, no options: EMPTY request (CoAP ping), ACK, RESET
        // 

        public MessageBuilder CreateAck( CoAPMessageRaw msg, MessageContext ctx )
        {
            return CreateResponse( msg, ctx )
                .WithNoOptions(                                         )
                .WithType     ( CoAPMessage.MessageType.Acknowledgement )
                .WithMessageId( msg.MessageId                           );
        }

        public MessageBuilder CreateResetResponse( CoAPMessageRaw msg, MessageContext ctx )
        {
            return CreateResponse( msg, ctx )
                .WithNoOptions  (                                  )
                .WithType       ( CoAPMessageRaw.MessageType.Reset )
                .WithMessageId  ( msg.MessageId                    )
                .WithRequestCode( CoAPMessage.Detail_Request.Empty );
        }

        //
        // Immediate and delayed responses: use ETag if available
        // 

        public MessageBuilder CreateImmediateResponse( CoAPMessageRaw msg, MessageContext ctx )
        {
            return CreateResponse( msg, ctx )
                .WithNoOptionsButETag(                                         )
                .WithType            ( CoAPMessage.MessageType.Acknowledgement )
                .WithMessageId       ( msg.MessageId                           )
                .WithCode            ( ctx.ResponseCode                        )
                .WithPayload         ( ctx.ResponsePayload                     )
                .WithOptions         ( ctx.ResponseOptions                     );
        }

        public MessageBuilder CreateDelayedResponse( CoAPMessageRaw msg, MessageContext ctx )
        {
            return CreateResponse( msg, ctx )
                .WithNoOptionsButETag(                     )
                .WithType            ( msg.Type            )
                .WithMessageId       (                     )
                .WithCode            ( ctx.ResponseCode    )
                .WithOptions         ( ctx.ResponseOptions )
                .WithPayload         ( ctx.ResponsePayload );
        }

        public MessageBuilder CreateOriginRequest( CoAPMessage msg )
        {
            var translatedOptions = new MessageOptions(); 

            foreach(var opt in msg.Options.Options)
            {
                if(opt.Number == MessageOption.OptionNumber.Uri_Path)
                {
                    if(opt.Value.Equals( Defaults.ProxyDirectory))
                    {
                        continue;
                    }
                }

                translatedOptions.Add( opt ); 
            }

            return this
                .WithHeader     ( msg.Header           )
                .WithTokenLength( Defaults.TokenLength )
                .WithOptions    ( translatedOptions    )
                .WithPayload    ( msg.Payload          );
        }

        //--//
        //--//
        //--//
        
        private MessageBuilder WithNoOptions( )
        {
            m_doNotUseOptions = NoOptions.None;

            return this;
        }

        private MessageBuilder WithNoOptionsButETag( )
        {
            m_doNotUseOptions = NoOptions.JustETag;

            return this;
        }

        public MessageBuilder WithETag( byte[] tag )
        {
            if(tag != null && tag.Length > 0)
            {
                var opt = MessageOption_Opaque.New( MessageOption.OptionNumber.ETag, tag );

                m_options.Add( opt );

                m_options.ETag = opt;
            }

            return this;
        }

        public MessageBuilder WithContext( MessageContext ctx )
        {
            m_context = ctx;

            return this;
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

        public MessageBuilder WithToken( MessageToken token )
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
            if(option != null)
            {
                m_options.Add( option );
            }
            
            return this;
        }

        public MessageBuilder WithOptions( MessageOptions options )
        {
            if(options == null)
            {
                options = MessageOptions.EmptyOptions;
            }

            foreach(var opt in options.Options)
            {
                m_options.Add( opt );
            }

            return this;
        }

        public MessageBuilder WithPersistentOptions( MessageOptions options )
        {
            if(options == null)
            {
                options = MessageOptions.EmptyOptions;
            }

            foreach(var opt in options.Options)
            {
                m_persistentOptions.Add( opt );
            }

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
            var msg = CoAPMessageRaw.NewBlankMessage( );
            
            try
            {
                if(m_doNotUseOptions == NoOptions.None)
                {
                    m_options = new MessageOptions( );
                }
                else
                {
                    if(m_doNotUseOptions == NoOptions.JustETag)
                    {
                        var etag = m_options.ETag;

                        m_options = new MessageOptions( );

                        if(etag != null)
                        {

                            m_options.Add( etag );
                        }
                    }
                    else
                    {
                        foreach(var option in m_persistentOptions.Options)
                        {
                            m_options.Add( option );
                        }
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
                var count = ComputeSize( );

                byte[] buffer = null;
                if(msg.Buffer.Length < count)
                {
                    buffer = new byte[ count ];
                }
                else
                {
                    buffer = msg.Buffer;
                }

                //
                // Complete the message
                //

                var stream = new NetworkOrderBinaryStream( buffer );

                stream.WriteUInt32( m_header );

                m_token.Encode( stream );
                m_options.Encode( stream );
                m_payload.Encode( stream );

                msg.Buffer  = buffer;     // Setting the buffer updates the heder in CoAPMessageRaw
                msg.Context = m_context;
            }
            finally
            {
                m_header  = 0; 
                m_token   = MessageToken.EmptyToken;
                m_options.Clear( );
                m_payload = MessagePayload.EmptyPayload;

                m_context         = null;
                m_doNotUseOptions = NoOptions.All;
            }

            return msg;
        }
        
        public override string ToString( )
        {
            return $"MESSAGE[HEADER({HeaderToString( )})](TOKEN({m_token}),OPTIONS({m_options}),PAYLOAD({m_payload}))";
        }

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
            return m_uniqueRandom.GetBytes( bytes );
        }

        public int NewETag( )
        {
            return m_uniqueRandom.GetInt( );
        }

        private int NewMessageId( )
        {
            return m_uniqueRandom.GetShort( );
        }

        public static int NewGlobalETag( )
        {
            return RandomNumberGenerator.GetInt( ); 
        }

        //--//

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
    }
}
