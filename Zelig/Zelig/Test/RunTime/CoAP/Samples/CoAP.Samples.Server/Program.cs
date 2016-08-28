//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Server
{
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;
    using System.Net;


    class Program
    {
        //
        // State
        // 
        private static BasicUdpServer s_server; 

        //--//

        static void Main( string[ ] args )
        {
            Logger.Instance = new ConsoleLogger( );
            
            var uri = new CoAPServerUri( new IPEndPoint( IPAddress.Parse( "10.0.1.28" ) , 11000 ), "res" ); 
            
            Logger.Instance.Log( $"Creating server @'{uri.Scheme}{uri.EndPoints[0]}/{uri.Path}'" ); 

            s_server = new BasicUdpServer( uri.EndPoints );
            
            Logger.Instance.Log( $"Activating" ); 

            s_server.Run( ); 
        }

        public static void Exit()
        {
            s_server.Exit( ); 
        }
    }
}
