//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;


    public static class Utils
    {
        public static bool ByteArrayCompare( byte[ ] buffer1, byte[ ] buffer2 )
        {
            if(buffer1.Length != buffer2.Length)
            {
                return false;
            }

            for(int i = 0; i < buffer1.Length; i++)
            {
                if(buffer1[ i ] != buffer2[ i ])
                {
                    return false;
                }
            }

            return true;
        }

        public static IPAddress AddressFromHostName( string hostName )
        {
            IPAddress[] addresses = Dns.GetHostEntry( hostName ).AddressList;
            
            foreach(var addr in addresses)
            {
                if(addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr;
                }
            }

            return null;
        }

        public static IPEndPoint[ ] EndPointsFromHostName( string hostName, int port )
        {
            IPAddress[] addresses = Dns.GetHostEntry( hostName ).AddressList;

            List<IPEndPoint> endPoints         = new List<IPEndPoint>();
            IPAddress[]      loopBackAddresses = new IPAddress[0];

            foreach(var addr in addresses)
            {
                if(addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ep = new IPEndPoint( addr, port );

                    endPoints.Add( ep );

                    //
                    // Also find out the IP address that the loopback address (127.0.0.1) maps to. 
                    // 
                    if(IPAddress.IsLoopback( addr ))
                    {
                        loopBackAddresses = Dns.GetHostEntry( addr ).AddressList;
                    }
                }
            }

            foreach(var addr in loopBackAddresses)
            {
                if(addr.AddressFamily == AddressFamily.InterNetwork && IPAddress.IsLoopback( addr ) == false)
                {
                    var ep = new IPEndPoint( addr, port );

                    endPoints.Add( ep );
                }
            }

            return endPoints.ToArray( );
        }
    }
}
