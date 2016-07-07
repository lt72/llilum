//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    public interface IUniqueRandom
    {
        byte[ ] GetBytes( int unique, byte[ ] bytes ); 

        ushort GetShort( int unique ); 
    }
}
