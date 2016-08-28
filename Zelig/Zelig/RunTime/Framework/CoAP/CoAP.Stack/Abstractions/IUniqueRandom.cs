//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public interface IUniqueRandom
    {
        byte[ ] GetBytes( byte[ ] bytes ); 

        ushort GetShort( );

        int GetInt( );
    }
}
