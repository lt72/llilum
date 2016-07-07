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
        private ICoAPChannel      m_channel;
        private IPEndPoint        m_remoteEndPoint; 
        private IPEndPoint        m_destinationEndPoint; 
        private uint              m_code;
        private IResourceHandler  m_resourceHandler;
        private byte[ ]           m_responsePayload;
        private CoAPMessageRaw    m_response;
        private CoAPMessage.Error m_error;

        //--//

        //
        // Contructors 
        //

        public MessageContext( CoAPMessageRaw message )
        {
            m_message = message;

            message.Context = this;
        }

        //
        // Helper methods
        // 


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
                CoAPMessage msg = CoAPMessage.FromBuffer( m_message.Buffer );

                using(var parser = MessageParser.CheckOutParser( ))
                {
                    parser.Parse( msg, m_channel.LocalEndPoint );
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

        public ICoAPChannel Channel
        {
            get
            {
                return m_channel;
            }
            set
            {
                m_channel = value;
            }
        }

        public IPEndPoint Source
        {
            get
            {
                return m_remoteEndPoint;
            }
            set
            {
                m_remoteEndPoint = (IPEndPoint)value;
            }
        }
        
        public IPEndPoint Destination
        {
            get
            {
                return m_destinationEndPoint;
            }
            internal set
            {
                m_destinationEndPoint = value;
            }
        }

        public IResourceHandler ResourceHandler
        {
            get
            {
                return m_resourceHandler;
            }
            set
            {
                m_resourceHandler = value;
            }
        }

        public byte[] ResponsePayload
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

        public CoAPMessageRaw Response
        {
            get
            {
                return m_response;
            }
            set
            {
                m_response = value;
            }
        }

        public uint ResponseCode
        {
            get
            {
                Debug.Assert( m_code <= 0xFF );

                return m_code;
            }
            set
            {
                Debug.Assert( value <= 0xFF );

                m_code = value;
            }
        }

        public CoAPMessage.Error Error
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
                    port = (int)(uint)opt.Value;

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
