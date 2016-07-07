//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define K64F

namespace Microsoft.Zelig.Test.mbed.SimpleNet
{
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using Microsoft.Llilum.Lwip;
    using Microsoft.Zelig.Runtime;

    using System.Text;
    using System;
    using CoAP.Common;
    using CoAP.Stack;
    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );

            BugCheck.Log( "Acquired IPv4 Address" );
            BugCheck.Log( netif.IPAddress.ToString( ) );

            //netif.EnableStaticIP("10.125.148.136", "255.255.254.0", "10.125.148.1");

            //IPHostEntry entry = Dns.GetHostEntry("bing.com");

            //IPAddress dnsAddress = entry.AddressList[0];

            //byte[] dnsCmd = new byte[entry.HostName.Length];

            //for(int i = 0; i < entry.HostName.Length; i++)
            //{
            //    dnsCmd[ i ] = (byte)entry.HostName[ i ];
            //}

            string msg = "GET /media/uploads/mbed_official/hello.txt HTTP/1.0\n\n";
            string end = "TEST_COMPLETED";
            var msgBytes = ASCIIEncoding.ASCII.GetBytes(msg);
            var endBytes = ASCIIEncoding.ASCII.GetBytes(end);

            var targetEndPoint = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), 8080 );

            IssueCoAPRequest( new ServerCoAPUri( targetEndPoint, "res/temperature/100" ), 8081 );

            // NOTE: Be sure to change this to your local machine IP that is running the NetworkTest app
            IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse("10.0.1.28"), 11000);

            var recBuffer = new byte[512];

            while(true)
            {
                Socket sock = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Unspecified);

                try
                {
                    sock.Connect( endPoint );

                    int count = 0;

                    while(++count <= 10)
                    {
                        if(sock.Send( msgBytes ) > 0)
                        {
                            sock.Receive( recBuffer );
                        }
                    }

                    sock.Send( endBytes );
                }
                catch
                {

                }
                finally
                {
                    sock.Close( );
                }
            }
        }

        private static void IssueCoAPRequest( CoAP.Stack.ServerCoAPUri uri, int localPort )
        {
            var localEndPoint = new IPEndPoint( IPAddress.Loopback, localPort );

            //
            // Create a client
            //

            var targetEndPoint = new IPEndPoint( CoAP.Common.Utils.AddressFromHostName( "localhost" ), 8080 );

            using(var client = new CoAP.Client.CoAPClient( new CoAP.Stack.Messaging( new CoAP.UdpTransport.UdpChannelFactory( ), localEndPoint ), new CoAP.Common.Diagnostics.Statistics( ) ))
            {
                //
                // Craft a request builder
                //

                var builder = client.Connect( null, uri );

                var request = builder
                        .WithVersion    ( CoAP.Stack.CoAPMessage.ProtocolVersion.Version_1 )
                        .WithType       ( CoAP.Stack.CoAPMessage.MessageType.Confirmable )
                        .WithTokenLength( CoAP.Common.Defaults.TokenLength )
                        .WithRequestCode( CoAP.Stack.CoAPMessage.Detail_Request.GET )
                        .WithOption     ( CoAP.Stack.MessageOption_String.New( CoAP.Stack.MessageOption.OptionNumber.Uri_Path, uri.Path ) )
                        .WithOption     ( CoAP.Stack.MessageOption_UInt.New( CoAP.Stack.MessageOption.OptionNumber.Max_Age , 30 ) )
                        .WithPayload    ( new byte[] { 0x01, 0x02, 0x03, 0x04 } )
                        .BuildAndReset( );

                var response = client.SendReceive( request );

                PrintResponse( response );
            }
        }

        private static void PrintResponse( CoAP.Stack.CoAPMessage response )
        {
            if(response == null)
            {
                System.Console.WriteLine( String.Format( $"***(C)*** Request was not fullfilled" ) );
            }
            else
            {
                System.Console.WriteLine( String.Format( $"== Request 'ID:{response.MessageId}' completed with result '{CoAP.Common.Defaults.Encoding.GetString( response.Payload.Payload )}' ==" ) );
            }
        }
    }
}
