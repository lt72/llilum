//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using CoAP.Stack;
    using CoAP.Common;


    internal class PingProvider : StandardResourceProvider
    {
        //
        // State
        //
        private int m_count;
        
        //
        // Contructors
        //

        internal PingProvider( )
        {
            m_count = 0;
        }

        public override bool IsImmediate
        {
            get
            {
                return true;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        //--//

        protected override uint GET( string query, out object result )
        {
            result = ++m_count;

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }
    }
}
