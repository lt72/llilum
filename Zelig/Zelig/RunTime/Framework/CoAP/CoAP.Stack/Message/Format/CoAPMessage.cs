//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using CoAP.Common;


    public class CoAPMessage : CoAPMessageRaw
    {
        //
        // State 
        // 
        
        private MessageOptions m_options;
        private MessagePayload m_payload;
        private bool           m_badOptions;

        //--//

        //
        // Contructors
        //

        private CoAPMessage( byte[ ] buffer ) : base( buffer )
        {
        }

        private CoAPMessage( ) : this( Constants.EmptyBuffer )
        {
        }
        
        //--//

        public static CoAPMessage FromBuffer( byte[ ] buffer )
        {
            return new CoAPMessage( buffer == null ? Constants.EmptyBuffer : buffer ); 
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
                       (this.ClassCode          == Class.Request              ) && // is a request 
                       (this.DetailCode_Request == Detail_Request.Empty       );   // is empty
            }
        }

        public bool IsPiggyBackedResponse
        {
            get
            {
                return (this.Type               == MessageType.Acknowledgement) && // ACK
                       (this.ClassCode          != Class.Request              ) && // not a request
                       (this.DetailCode_Request != Detail_Request.Empty       );   // not empty
            }
        }

        public bool IsDelayedResponse
        {
            get
            {
                return ((this.Type               == MessageType.Confirmable   )  ||
                        (this.Type               == MessageType.NonConfirmable)) && // CON or NON (not an ACK)
                        (this.ClassCode          != Class.Request              ) && // not a request
                        (this.DetailCode_Request != Detail_Request.Empty       );   // not empty
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

        public bool IsPing
        {
            get
            {
                return (this.Type               == MessageType.Confirmable) && // CON  
                       (this.ClassCode          == Class.Request          ) && // is a request
                       (this.DetailCode_Request == Detail_Request.Empty   );   // is empty;
            }
        }


#if DESKTOP
        public override string ToString( )
        {
            return $"{base.ToString( )},OPTIONS({this.Options}),PAYLOAD({this.Payload}))";
        }
#endif

        //--//

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

        public int Length
        {
            get
            {
                return m_buffer.Length;
            }
        }

        public bool HasBadOptions
        {
            get
            {
                return m_badOptions;
            }
            set
            {
                m_badOptions = value;
            }
        }

        //--//
    }
}