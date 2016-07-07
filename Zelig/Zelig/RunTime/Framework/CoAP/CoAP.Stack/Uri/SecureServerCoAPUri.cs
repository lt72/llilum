//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    using System.Net;


    public class SecureServerCoAPUri : ServerCoAPUri
    {
        //
        // State
        //

        //--//

        //
        // Constructors
        //

        public SecureServerCoAPUri( IPEndPoint[ ] endPoints, string path ) : base( Scheme__CoAP, endPoints, path )
        {
        }

        public SecureServerCoAPUri( IPEndPoint endPoint, string path ) : base( Scheme__Secure_CoAP, endPoint, path )
        {
        }

        //--//

        //
        // Access methods
        //

    }
}
