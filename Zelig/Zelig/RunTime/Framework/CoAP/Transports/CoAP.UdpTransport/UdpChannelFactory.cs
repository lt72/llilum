//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.UdpTransport
{
    using System.Net;
    using CoAP.Stack.Abstractions;


    public class UdpChannelFactory : IChannelFactory
    {
        //
        // State
        //
                
        //--//
        
        //
        // Contructors
        // 
        public UdpChannelFactory( )
        {
        }
        
        public ICoAPChannel Create( IPEndPoint endPoint, bool fBind )
        {
            var ch = new UdpChannel( endPoint );

            if(fBind)
            {
                ch.Bind( endPoint ); 
            }

            return ch;
        }

        public void Retire( ICoAPChannel channel )
        {
            channel.Close( ); 
        }
    }
}
