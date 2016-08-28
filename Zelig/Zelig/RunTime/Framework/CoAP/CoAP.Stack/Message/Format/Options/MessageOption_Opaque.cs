//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using CoAP.Common;


    public class MessageOption_Opaque : MessageOption
    {

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_Opaque( OptionNumber option, byte[] value ) : base(option, value)
        {
        }

        public static MessageOption_Opaque New( OptionNumber number, byte[ ] value )
        {
            switch(number)
            {
                case OptionNumber.If_Match:
                case OptionNumber.If_None_Match:
                case OptionNumber.ETag:
                    return new MessageOption_Opaque( number, value );

                default:
                    return new MessageOption_Opaque( number, value ); // unrecognized option
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
            return $"{this.Name}({Utils.ByteArrayPrettyPrint( this.RawBytes )})";
        }

        //
        // Access Methods
        //

    }
}
