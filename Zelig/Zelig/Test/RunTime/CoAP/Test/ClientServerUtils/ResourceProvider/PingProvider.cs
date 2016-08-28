//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using CoAP.Stack;
    using CoAP.Server;

    public sealed class PingProvider : ResourceProvider
    {
        //
        // State
        //
        private int m_count;

        //
        // Contructors
        //

        public PingProvider( ) : base( true, false )
        {
            m_count = 0;
        }

        public override bool CanFetchImmediateResponse( CoAPMessage request )
        {
            return true;
        }

        protected override uint GET( string path, string[ ] query, out MessagePayload payload )
        {
            payload = MessagePayload_Int.New( ++m_count );

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }
    }
}
