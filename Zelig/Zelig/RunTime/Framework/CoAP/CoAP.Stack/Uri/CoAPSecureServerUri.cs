//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System.Net;


    public class CoAPSecureServerUri : CoAPServerUri
    {
        //
        // State
        //

        //--//

        //
        // Constructors
        //

        public CoAPSecureServerUri( IPEndPoint[ ] endPoints, string path ) : base( Scheme__CoAP, endPoints, path )
        {
        }

        public CoAPSecureServerUri( IPEndPoint endPoint, string path ) : base( Scheme__Secure_CoAP, endPoint, path )
        {
        }

        //--//

        //
        // Access methods
        //

    }
}
