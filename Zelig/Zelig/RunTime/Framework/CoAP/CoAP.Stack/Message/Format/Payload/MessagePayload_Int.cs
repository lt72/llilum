//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Text;
    using CoAP.Common;


    public sealed class MessagePayload_Int : MessagePayload
    {
        //
        // State
        // 
        
        //--//

        //
        // Contructors
        //

        public MessagePayload_Int( int payload ) : base( Utils.ByteArrayFromInteger( payload ) )
        {
        }
        
        public static MessagePayload New( int value )
        {
            return new MessagePayload_Int( value );
        }

        //
        // Helper methods
        // 

        public override string ToString( )
        {
            return this.Value.ToString( );
        }

        //
        // Access methods
        // 

        public override object Value
        {
            get
            {
                return Utils.ByteArrayToInteger( this.RawBytes );
            }
        }
    }
}