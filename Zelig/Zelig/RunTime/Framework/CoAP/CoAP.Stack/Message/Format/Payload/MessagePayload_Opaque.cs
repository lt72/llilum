//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Text;
    using CoAP.Common;


    public sealed class MessagePayload_Opaque : MessagePayload
    {
        //
        // State
        // 
        
        //--//

        //
        // Contructors
        //

        private MessagePayload_Opaque( byte[ ] payload ) : base( payload )
        {
        }

        public static MessagePayload New( byte[ ] raw )
        {
            return new MessagePayload_Opaque( raw );
        }


        //
        // Helper methods
        // 

        public override string ToString( )
        {
            return Utils.ByteArrayPrettyPrint( this.RawBytes );
        }

        //--//

        //
        // Access methods
        // 

        public override object Value
        {
            get
            {
                return this.RawBytes; 
            }
        }
    }
}