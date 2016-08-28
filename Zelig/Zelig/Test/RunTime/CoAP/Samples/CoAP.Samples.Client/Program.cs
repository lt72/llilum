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
            Logger.Instance = new ConsoleLogger( );

            new Thread( IssueRequest100 ).Start( );
            new Thread( IssueRequest200 ).Start( );

            Thread.Sleep( Timeout.Infinite ); 
        }


        //--//
        
        private static void IssueRequest100( )
        {
            var targetEndPoint = new IPEndPoint( IPAddress.Parse("10.0.1.3"), 8080 );

            IssueRequest( new CoAPServerUri( targetEndPoint, "res/temperature/100" ), 8081 ); 
        }
        
        private static void IssueRequest200( )
        {
            var targetEndPoint = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), 8080 );

            IssueRequest( new CoAPServerUri( targetEndPoint, "res/temperature/200" ), 8082 );
        }

        private static void IssueRequest( CoAPServerUri uri, int localPort )
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
                    .WithOption     ( MessageOption_Int.New( MessageOption.OptionNumber.Max_Age , 30 ) )
                    .WithPayload    ( MessagePayload_Opaque.New( new byte[] { 0x01, 0x02, 0x03, 0x04 } ) )
                    .Build( );
                
                var response = client.MakeRequest( request );

                PrintResponse( response ); 
            }

        }

        private static void PrintResponse( CoAPMessage response )
        {
            if(response == null)
            {
                Logger.Instance.LogError( String.Format( $"***(C)*** Request was not fullfilled" ) );
            }
            else
            {
                Logger.Instance.Log( String.Format( $"== Request 'ID:{response.MessageId}' completed with result '{response.Payload}' ==" ) );
            }
        }
    }
}
