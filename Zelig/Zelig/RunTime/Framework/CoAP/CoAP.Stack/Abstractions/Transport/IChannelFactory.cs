//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    using System.Net;


    public interface IChannelFactory
    {
        ICoAPChannel Create( IPEndPoint endPoint, bool fBind );

        void Retire( ICoAPChannel channel );
    }
}
