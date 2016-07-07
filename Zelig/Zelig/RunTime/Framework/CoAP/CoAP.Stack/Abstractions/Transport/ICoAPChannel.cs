//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack.Abstractions
{
    using System.Net;


    public interface ICoAPChannel
    {
        void Open( IPEndPoint endPoint );
        
        void Close( );

        void Bind( EndPoint endPoint ); 

        int Send( byte[] buffer, int offset, int count, EndPoint receiver ); 

        int Receive( byte[] buffer, int offset, int count, ref EndPoint sender ); 

        int Available { get; }
        
        int SendTimeout { get; set; }

        int ReceiveTimeout { get; set; }

        IPEndPoint LocalEndPoint { get; }
    }
}