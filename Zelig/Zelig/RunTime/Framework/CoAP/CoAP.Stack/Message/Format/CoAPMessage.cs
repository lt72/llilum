//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using CoAP.Common;

    public class CoAPMessage : CoAPMessageRaw
    {
        //
        // State 
        // 
        
        private MessageOptions m_options;
        private MessagePayload m_payload;

        //--//

        //
        // Contructors
        //

        private CoAPMessage( byte[ ] buffer ) : base( buffer )
        {
            this.Buffer = buffer;
        }

        private CoAPMessage( ) : this( Constants.EmptyBuffer )
        {
        }

        //--//

        internal static CoAPMessage FromBuffer( byte[ ] buffer )
        {
            Debug.Assert( buffer != null && buffer.Length >= 4 ); 

            return new CoAPMessage( buffer );
        }

        public static CoAPMessage FromBufferWithContext( byte[ ] buffer, MessageContext ctx )
        {
            var msg = new CoAPMessage( buffer == null ? Constants.EmptyBuffer : buffer );

            msg.Context = ctx;

            return msg;
        }

        public static bool ParseFromBuffer( byte[ ] buffer, MessageParser parser, ref CoAPMessage msg )
        {
            return parser.Parse( buffer, ref msg );
        }

        public static bool ParseFromBufferWithDestination( byte[ ] buffer, MessageParser parser, IPEndPoint ep, ref CoAPMessage msg )
        {
            return parser.ParseAndComputeDestination( buffer, ep, ref msg );
        }

        //
        // Helper methods
        //

        public override string ToString( )
        {
            return $"MESSAGE[{base.ToString( false )},OPTIONS({this.Options}),PAYLOAD({this.Payload}))]";
        }

        //
        // Access methods
        //

        public MessageOptions Options
        {
            get
            {
                return m_options;
            }
            set
            {
                m_options = value;
            }
        }

        public MessagePayload Payload
        {
            get
            {
                return m_payload;
            }
            set
            {
                m_payload = value;
            }
        }
        
        public bool IsTagged
        {
            get
            {
                //return Object.ReferenceEquals( m_options.ETag, MessageOptions.EmptyETag ) == false;
                return m_options.ETag != null;
            }
        }
    }
}