//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;


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

        public static int ByteArrayToHash( byte[ ] buffer )
        {
            int hash = 0;

            for(int i = 0; i < buffer.Length; i++)
            {
                hash ^= (buffer[ i ] << (i % 4)); 
            }

            return hash;
        }

        public static byte[ ] ByteArrayFromInteger( int payload )
        {
            return new byte[ 4 ]
            {
                (byte)((payload & 0xFF000000 ) >> 24),
                (byte)((payload & 0x00FF0000 ) >> 16),
                (byte)((payload & 0x0000FF00 ) >> 8 ),
                (byte)((payload & 0x000000FF )      ),
            };
        }

        public static int ByteArrayToInteger( byte[ ] payload )
        {
            Debug.Assert( payload.Length == sizeof(int) );

            int res = 0;

            res |= payload[ 0 ] << 24;
            res |= payload[ 1 ] << 16;
            res |= payload[ 2 ] <<  8;
            res |= payload[ 3 ]      ;

            return res;
        }

        public static byte[ ] ByteArrayFromString( string payload )
        {
            return Defaults.Encoding.GetBytes( payload );
        }

        public static string ByteArrayToString( byte[ ] payload )
        {
            return Defaults.Encoding.GetString( payload );
        }

        public static string ByteArrayPrettyPrint( byte[ ] buffer )
        {
            var sb = new StringBuilder();

            for(int i = 0; i < buffer.Length; i++)
            {
                sb.Append( $"0x{buffer[ i ]:X}" );

                if(i < buffer.Length - 1)
                {
                    sb.Append( "," );
                }
            }

            return $"{sb}";
        }

        //--//
        //--//
        //--//

        public static IPAddress AddressFromHostName( string hostName )
        {
            IPAddress address = null;
            if(IPAddress.TryParse( hostName, out address ))
            {
                return address;
            }

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
