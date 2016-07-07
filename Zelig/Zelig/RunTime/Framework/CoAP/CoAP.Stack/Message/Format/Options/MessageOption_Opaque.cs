//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System.Text;


    public class MessageOption_Opaque : MessageOption
    {

        //
        // State
        //

        private readonly byte[] m_value; 

        //--//

        //
        // Contructors
        //
        
        internal MessageOption_Opaque( OptionNumber option, byte[] value ) : base(option)
        {
            m_value = value;
        }

        public static MessageOption New( OptionNumber number, byte[ ] value )
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

            stream.WriteBytes( m_value, 0, m_value.Length );
        }

#if DESKTOP
        public override string ToString( )
        {
            var sb = new StringBuilder();

            for(int i = 0; i < m_value.Length; i++)
            {
                sb.Append( $"0x{m_value[ i ]:X}" ); 
                
                if(i < m_value.Length - 1)
                {
                    sb.Append( "," ); 
                }
            }
            
            return $"{this.Name}({sb})";
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
