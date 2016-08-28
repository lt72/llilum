//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Text;
    using CoAP.Common;


    public sealed class MessagePayload_String : MessagePayload
    {
        //
        // State
        // 
        
        //--//

        //
        // Contructors
        //

        private MessagePayload_String( string payload ) : base( Utils.ByteArrayFromString( payload ) )
        {
        }


        //
        // Helper methods
        // 

        public override string ToString( )
        {
            return Utils.ByteArrayToString( this.RawBytes ); 
        }

        public static MessagePayload New( string s )
        {
            return new MessagePayload_String( s );
        }

        //
        // Access methods
        // 

        public override object Value
        {
            get
            {
                return ToString( ); 
            }
        }

    }
}