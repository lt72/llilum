//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Client
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;


    class Program
    {

        //
        // State
        // 

        //--//

        static void Main( string[ ] args )
        {
            
            new Thread( IssueRequest100 ).Start( );
            new Thread( IssueRequest200 ).Start( );

            Thread.Sleep( Timeout.Infinite ); 
        }


        //--//
        
        private static void IssueRequest100( )
        {
            var targetEndPoint = new IPEndPoint( IPAddress.Parse("10.0.1.3"), 8080 );

            IssueRequest( new ServerCoAPUri( targetEndPoint, "res/temperature/100" ), 8081 ); 
        }
        
        private static void IssueRequest200( )
        {
            var targetEndPoint = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), 8080 );

            IssueRequest( new ServerCoAPUri( targetEndPoint, "res/temperature/200" ), 8082 );
        }

        private static void IssueRequest( ServerCoAPUri uri, int localPort )
        {
            var localEndPoint = new IPEndPoint( IPAddress.Loopback, localPort );

            //
            // Create a client
            //
            var client = new BasicUdpClient( localEndPoint );

            //
            // Craft a request builder
            //

            var builder = client.Connect( null, uri );
            
            while(true)
            {
                var request = builder
                    .WithVersion    ( CoAPMessage.ProtocolVersion.Version_1 )
                    .WithType       ( CoAPMessage.MessageType.Confirmable )
                    .WithTokenLength( Defaults.TokenLength )
                    .WithRequestCode( CoAPMessage.Detail_Request.GET )
                    .WithOption     ( MessageOption_String.New( MessageOption.OptionNumber.Uri_Path, uri.Path ) )
                    .WithOption     ( MessageOption_UInt.New( MessageOption.OptionNumber.Max_Age , 30 ) )
                    .WithPayload    ( new byte[] { 0x01, 0x02, 0x03, 0x04 } )
                    .BuildAndReset( );
                
                var response = client.MakeRequest( request );

                PrintResponse( response ); 
            }

        }

        private static void PrintResponse( CoAPMessage response )
        {
            if(response == null)
            {
                Logger.LogError( String.Format( $"***(C)*** Request was not fullfilled" ) );
            }
            else
            {
                Logger.Log( String.Format( $"== Request 'ID:{response.MessageId}' completed with result '{Defaults.Encoding.GetString( response.Payload.Payload )}' ==" ) );
            }
        }
    }
}
