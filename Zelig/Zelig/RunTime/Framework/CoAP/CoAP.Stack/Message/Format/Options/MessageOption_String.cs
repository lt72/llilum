//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using CoAP.Common;


    public sealed class MessageOption_String : MessageOption
    {

        //
        // State
        //

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_String( OptionNumber option, string value ) : base(option, Utils.ByteArrayFromString( value ))
        {
        }

        public static MessageOption_String New( OptionNumber number, string value )
        {
            switch(number)
            {
                case OptionNumber.Uri_Path:
                case OptionNumber.Uri_Query:
                    Validate( value );
                    return new MessageOption_String( number, value );
                case OptionNumber.Uri_Host:
                case OptionNumber.Location_Path:
                case OptionNumber.Location_Query:
                case OptionNumber.Proxy_Uri:
                case OptionNumber.Proxy_Scheme:
                    return new MessageOption_String( number, value );

                default:
                    throw new CoAP_MessageMalformedException( );
            }
        }

        private static void Validate( string value )
        {
            if(value.IndexOfAny( CoAPUri.AllUriDelimiters ) != - 1)
            {
                throw new ArgumentException( ); 
            }
        }

        //
        // Helper Methods
        // 

        public override void Encode( NetworkOrderBinaryStream stream )
        {
            base.Encode( stream ); 

            stream.WriteBytes( this.RawBytes, 0, this.ValueLength );
        }

        public override string ToString( )
        {
            return $"{this.Name}('{Utils.ByteArrayToString( this.RawBytes )}')";
        }

        //
        // Access Methods
        //

        public override object Value
        {
            get
            {
                return Utils.ByteArrayToString( this.RawBytes );
            }
        }
    }
}

