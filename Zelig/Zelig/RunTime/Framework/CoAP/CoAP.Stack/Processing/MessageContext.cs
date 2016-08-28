//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System.Net;
    using System.Collections.Generic;
    using System.Diagnostics;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;


    public class MessageContext 
    {
        //
        // State
        // 

        private CoAPMessageRaw    m_message;
        private IPEndPoint        m_sourceEndPoint;
        private IPEndPoint        m_destinationEndPoint;
        private CoAPMessageRaw    m_responseAwaitingAck;
        private uint              m_responseCode;
        private MessageOptions    m_responseOptions;
        private MessagePayload    m_responsePayload;
        private CoAPMessage.Error m_error;

        //--//

        //
        // Contructors 
        //

        private MessageContext( CoAPMessageRaw message )
        {
            m_message = message;

            m_responseOptions = new MessageOptions( ); 
        }
        
        //--//

        public static MessageContext WrapWithContext( CoAPMessageRaw msg )
        {
            var ctx = new MessageContext( msg );
            
            ctx.Source      = msg.Context?.Source;
            ctx.Destination = msg.Context?.Destination;

            msg.Context = ctx;

            return ctx;
        }

        //
        // Helper methods
        // 

        public override string ToString( )
        {
            return $"CTX[TYPE={this.Message.Type},ID={m_message.MessageId},SRC={m_sourceEndPoint},DST={m_destinationEndPoint}]";
        }
        
        //
        // Access methods
        // 

        public CoAPMessage MessageInflated
        {
            get
            {
                if(m_message is CoAPMessage)
                {
                    return (CoAPMessage)m_message;
                }

                //
                // Create an inflated message
                //
                CoAPMessage msg = CoAPMessage.FromBufferWithContext( m_message.Buffer, this );

                bool fCorrect = false;
                using(var parser = MessageParser.CheckOutParser( ))
                {
                    fCorrect = parser.ParseAndComputeDestination( msg.Buffer, m_destinationEndPoint, ref msg );
                }

                return msg;
            }
        }

        public CoAPMessageRaw Message
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;

                m_message.Context = this;
            }
        }
        
        public IPEndPoint Source
        {
            get
            {
                return m_sourceEndPoint;
            }
            set
            {
                m_sourceEndPoint = (IPEndPoint)value;
            }
        }

        public IPEndPoint Destination
        {
            get
            {
                return m_destinationEndPoint;
            }
            set
            {
                m_destinationEndPoint = value;
            }
        }

        public MessagePayload ResponsePayload
        {
            get
            {
                return m_responsePayload;
            }
            set
            {
                m_responsePayload = value;
            }
        }

        public CoAPMessageRaw ResponseAwaitingAck
        {
            get
            {
                return m_responseAwaitingAck;
            }
            set
            {
                m_responseAwaitingAck = value;
            }
        }

        public uint ResponseCode
        {
            get
            {
                Debug.Assert( m_responseCode <= 0xFF );

                return m_responseCode;
            }
            set
            {
                Debug.Assert( value <= 0xFF );

                m_responseCode = value;
            }
        }

        public MessageOptions ResponseOptions
        {
            get
            {
                return m_responseOptions;
            }
        }

        public CoAPMessage.Error ProtocolError
        {
            get
            {
                return m_error;
            }
            set
            {
                m_error = value;
            }
        }
        
        //--//

        public static IPEndPoint ComputeDestination( LinkedList<MessageOption> options, IPEndPoint defaultEndPoint )
        {
            IPEndPoint destinationEndPoint = null;
            IPAddress  host                = defaultEndPoint?.Address;
            int        port                = defaultEndPoint != null ? defaultEndPoint.Port : CoAPUri.DefaultPort;

            bool fOriginServer = false;
            foreach(var opt in options)
            {
                if(opt.IsProxyUri)
                {
                    destinationEndPoint = CoAPUri.EndPointFromUri( (string)opt.Value );

                    //
                    // Proxy-Uri takes precedence
                    // 
                    break;
                }
                else if(opt.IsHostUri)
                {
                    host = Utils.AddressFromHostName( (string)opt.Value );

                    fOriginServer = true;
                }
                else if(opt.IsPort)
                {
                    port = (int)opt.Value;

                    fOriginServer = true;
                }
            }

            if(destinationEndPoint == null)
            {
                //
                // If there was any specification for targeting a proxy, use it...
                //
                if(fOriginServer)
                {
                    destinationEndPoint = new IPEndPoint( host, port );
                }
                else
                {
                    destinationEndPoint = defaultEndPoint;
                }
            }

            return destinationEndPoint;
        }
    }
}
