//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;


    public class MessageOption_String : MessageOption
    {

        //
        // State
        //

        private readonly string m_value; 

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_String( OptionNumber option, string value ) : base(option)
        {
            if(value.Length < 1 || value.Length > 255)
            {
                throw new ArgumentException( ); 
            }

            m_value = value;
        }

        public static MessageOption New( OptionNumber number, string value )
        {
            switch(number)
            {
                case OptionNumber.Uri_Host:
                case OptionNumber.Location_Path:
                case OptionNumber.Uri_Path:
                case OptionNumber.Uri_Query:
                case OptionNumber.Location_Query:
                case OptionNumber.Proxy_Uri:
                case OptionNumber.Proxy_Scheme:
                    return new MessageOption_String( number, value );

                default:
                    throw new CoAP_MessageFormatException( );
            }
        }

        //
        // Helper Methods
        // 

        public override void Encode( NetworkOrderBinaryStream stream )
        {
            base.Encode( stream ); 

            stream.WriteString( m_value, Common.Defaults.Encoding );
        }

#if DESKTOP
        public override string ToString( )
        {
            return $"{this.Name}('{m_value}')";
        }
#endif

        //
        // Access Methods
        //

        public override object Value
        {
            get
            {
                return m_value;
            }
        }

        public override int ValueLength
        {
            get
            {
                return m_value.Length;
            }
        }
    }
}

