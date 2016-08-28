//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using CoAP.Common;


    public sealed class MessageOption_Int : MessageOption
    {

        //
        // State
        //

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_Int( OptionNumber option, int value ) : base(option, Utils.ByteArrayFromInteger( value ))
        {
        }

        public static MessageOption New( OptionNumber number, MessageOption.ContentFormat value )
        {
            switch(number)
            {
                case OptionNumber.Accept        :
                case OptionNumber.Content_Format:
                    return new MessageOption_Int( number, (int)value );

                default:
                    throw new CoAP_MessageMalformedException( );
            }
        }

        public static MessageOption_Int New( OptionNumber number, int value )
        {
            switch(number)
            {
                case OptionNumber.Uri_Port      :
                case OptionNumber.Content_Format:
                case OptionNumber.Max_Age       :
                case OptionNumber.Accept        :
                case OptionNumber.Size1         :
                    return new MessageOption_Int( number, value );

                default:
                    throw new CoAP_MessageMalformedException( );
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
            return $"{this.Name}({Utils.ByteArrayToInteger( this.RawBytes )})";
        }

        //
        // Access Methods
        //

        public override object Value
        {
            get
            {
                return Utils.ByteArrayToInteger( this.RawBytes );
            }
        }

        public override int ValueLength
        {
            get
            {
                return sizeof( uint );
            }
        }
    }
}

