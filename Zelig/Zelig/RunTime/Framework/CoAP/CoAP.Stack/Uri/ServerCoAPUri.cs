//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System.Net;

    public class ServerCoAPUri : CoAPUri
    {
        //
        // State
        //

        //--//

        //
        // Constructors
        //

        protected ServerCoAPUri( string scheme, IPEndPoint[] endPoints, string path ) : base( scheme, endPoints, path )
        {
        }

        protected ServerCoAPUri( string scheme, IPEndPoint endPoint, string path ) : base( scheme, endPoint, path )
        {
        }

        public ServerCoAPUri( IPEndPoint[] endPoints, string path ) : base( Scheme__CoAP, endPoints, path )
        {
        }

        public ServerCoAPUri( IPEndPoint endPoint, string path ) : base( Scheme__CoAP, endPoint, path )
        {
        }

        //--//

        //
        // Access methods
        //

    }
}
