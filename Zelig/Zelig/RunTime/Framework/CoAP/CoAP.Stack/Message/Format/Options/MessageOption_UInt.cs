//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    public class MessageOption_UInt : MessageOption
    {

        //
        // State
        //

        private readonly uint m_value; 

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_UInt( OptionNumber option, uint value ) : base(option)
        {
            m_value = value;
        }

        public static MessageOption New( OptionNumber number, MessageOption.ContentFormat value )
        {
            switch(number)
            {
                case OptionNumber.Accept        :
                case OptionNumber.Content_Format:
                    return new MessageOption_UInt( number, (uint)value );

                default:
                    throw new CoAP_MessageFormatException( );
            }
        }

        public static MessageOption New( OptionNumber number, uint value )
        {
            switch(number)
            {
                case OptionNumber.Uri_Port      :
                case OptionNumber.Content_Format:
                case OptionNumber.Max_Age       :
                case OptionNumber.Accept        :
                case OptionNumber.Size1         :
                    return new MessageOption_UInt( number, value );

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
            
            stream.WriteUInt32( m_value ); 
        }

#if DESKTOP
        public override string ToString( )
        {
            return $"{this.Name}({m_value})";
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
                return sizeof( uint );
            }
        }
    }
}

