//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace CoAP.Stack
{
    public interface IEncodable
    {
        void Encode( NetworkOrderBinaryStream stream );
    }
}