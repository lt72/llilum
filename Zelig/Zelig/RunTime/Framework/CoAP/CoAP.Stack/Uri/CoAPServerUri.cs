//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System.Net;

    public class CoAPServerUri : CoAPUri
    {
        //
        // State
        //

        //--//

        //
        // Constructors
        //

        protected CoAPServerUri( string scheme, IPEndPoint[] endPoints, string path ) : base( scheme, endPoints, path )
        {
        }

        protected CoAPServerUri( string scheme, IPEndPoint endPoint, string path ) : base( scheme, endPoint, path )
        {
        }

        public CoAPServerUri( IPEndPoint[] endPoints, string path ) : base( Scheme__CoAP, endPoints, path )
        {
        }

        public CoAPServerUri( IPEndPoint endPoint, string path ) : base( Scheme__CoAP, endPoint, path )
        {
        }

        //--//

        //
        // Access methods
        //

    }
}
