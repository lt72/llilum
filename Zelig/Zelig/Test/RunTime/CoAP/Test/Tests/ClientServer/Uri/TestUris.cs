//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#undef DEBUG

namespace Microsoft.SPOT.Platform.Tests
{
    using Microsoft.Zelig.Test;
    using System.Net;
    using CoAP.Common;
    using CoAP.Stack;


    public class TestUris : CoApTestBase
    {
        //
        // coaps-URI = "coaps:" "//" host [ ":" port ] path-abempty [ "?" query ]
        //

        [SetUp]
        public override InitializeResult Initialize( )
        {
            Log.Comment( "*** Initialize test..." );

            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public override void CleanUp( )
        {
            Log.Comment( "*** Cleaning up after the tests" );
            Log.NewLine( );
            Log.NewLine( );
        }

        public override TestResult Run( string[ ] args )
        {
            TestResult res = TestResult.Pass;
            
            res |= UriToOptions( );

            return res;
        }

        [TestMethod]
        public TestResult UriToOptions( )
        {
            Log.Comment( "*** Convert a set of standard Uris to options." );

            var testUris = new string[]
            {
                "coap://google.com",
                "coap://google.com:",
                "coap://google.com:/",
                "coap://google.com:5683",
                "coap://google.com:5683/",

                "coap://google.com:5683/~sensors/temp.xml",
                "coap://google.com/%7Esensors/temp.xml",
                "coap://google.com:/%7esensors/temp.xml",

                "coaps://google.com:1/~sensors/temp.xml",
                "coaps://google.com:12/%7Esensors/temp.xml",
                "coaps://google.com:123/%7esensors/temp.xml",
                "coaps://google.com:1234/%7esensors/temp.xml",
                "coaps://google.com:12345/%7esensors/temp.xml",

                "coap://google.com:5683/~sensors/temp.xml?sensor1",
                "coap://google.com/%7Esensors/temp.xml?sensor1",
                "coap://google.com:/%7esensors/temp.xml?sensor1",

                "coap://google.com:5683/~sensors/temp.xml?sensor1&sensor2",
                "coap://google.com/%7Esensors/temp.xml?sensor1&sensor2&sensor3",
                "coap://google.com:/%7esensors/temp.xml?sensor1&sensor2&sensor3&sensor4",

                "coap://google.com:5683/~sensors/temperature/temp.xml",
                "coap://google.com/%7Esensors/temperature/temp.xml",
                "coap://google.com:/%7esensors/temperature/temp.xml",


                "coap://google.com:5683/~sensors/temperature/temp.xml?sensor1",
                "coap://google.com/%7Esensors/temperature/temp.xml?sensor1",
                "coap://google.com:/%7esensors/temperature/temp.xml?sensor1",
            };

            var testSecureUris = new string[]
            {
                "coaps://google.com",
                "coaps://google.com:",
                "coaps://google.com:/",
                "coaps://google.com:5683",
                "coaps://google.com:5683/",

                "coaps://google.com:5683/~sensors/temp.xml",
                "coaps://google.com/%7Esensors/temp.xml",
                "coaps://google.com:/%7esensors/temp.xml",

                "coaps://google.com:1/~sensors/temp.xml",
                "coaps://google.com:12/%7Esensors/temp.xml",
                "coaps://google.com:123/%7esensors/temp.xml",
                "coaps://google.com:1234/%7esensors/temp.xml",
                "coaps://google.com:12345/%7esensors/temp.xml",

                "coaps://google.com:5683/~sensors/temp.xml?sensor1",
                "coaps://google.com/%7Esensors/temp.xml?sensor1",
                "coaps://google.com:/%7esensors/temp.xml?sensor1",

                "coaps://google.com:5683/~sensors/temp.xml?sensor1&sensor2",
                "coaps://google.com/%7Esensors/temp.xml?sensor1&sensor2&sensor3",
                "coaps://google.com:/%7esensors/temp.xml?sensor1&sensor2&sensor3&sensor4",

                "coaps://google.com:5683/~sensors/temperature/temp.xml",
                "coaps://google.com/%7Esensors/temperature/temp.xml",
                "coaps://google.com:/%7esensors/temperature/temp.xml",

                "coaps://google.com:5683/~sensors/temperature/temp.xml?sensor1",
                "coaps://google.com/%7Esensors/temperature/temp.xml?sensor1",
                "coaps://google.com:/%7esensors/temperature/temp.xml?sensor1",
            };

            var testBadUris = new string[]
            {
                "google.com:5683/~sensors/temp.xml",
                "coap://google.com:a/~sensors/temp.xml",
                "coap://google.com:aa/~sensors/temp.xml",
                "coap://google.com:aaa/~sensors/temp.xml",
                "coap://google.com:aaaa/~sensors/temp.xml",
                "coap://google.com:aaaaa/~sensors/temp.xml",
                "coaps://google.com:a/~sensors/temp.xml",
                "coaps://google.com:aa/~sensors/temp.xml",
                "coaps://google.com:aaa/~sensors/temp.xml",
                "coaps://google.com:aaaa/~sensors/temp.xml",
                "coaps://google.com:aaaaa/~sensors/temp.xml",
                "coap://google.com:a/~sensors/temp.xml?",
            };

            var testEndpoint                = new IPEndPoint( Utils.AddressFromHostName( "google.com" ) , 5683 );
            var testSecureEndpoint          = new IPEndPoint( Utils.AddressFromHostName( "google.com" ) , 5684 );
            var testEndpointDifferent       = new IPEndPoint( Utils.AddressFromHostName( "goggles.com" ), 12345 );
            var testSecureEndpointDifferent = new IPEndPoint( Utils.AddressFromHostName( "goggles.com" ), 54321 );

            Log.Comment( "Non secure URIs" );
            Log.Comment( "" );

            foreach(var uri in testUris)
            {
                string scheme = null, host = null, path = null;
                int port = 0;
                var options = new MessageOptions();

                Log.Comment( $"URI: '{uri}" );

                CoAPUri.UriToComponents( uri, testEndpoint, out scheme, out host, out port, out path, options );

                Log.Comment( $"SCHEME: {scheme}, HOST: '{host}', PORT: '{port}', PATH: {path}, OPTIONS: {options}" );
                Log.Comment( "" );
            }

            foreach(var uri in testUris)
            {
                string scheme = null, host = null, path = null;
                int port = 0;
                var options = new MessageOptions();

                Log.Comment( $"URI: '{uri}" );

                CoAPUri.UriToComponents( uri, testEndpointDifferent, out scheme, out host, out port, out path, options );

                Log.Comment( $"SCHEME: {scheme}, HOST: '{host}', PORT: '{port}', PATH: {path}, OPTIONS: {options}" );
                Log.Comment( "" );
            }

            Log.Comment( "" );
            Log.Comment( "" );
            Log.Comment( "Secure URIs" );
            Log.Comment( "" );

            foreach(var uri in testSecureUris)
            {
                string scheme = null, host = null, path = null;
                int port = 0;
                var options = new MessageOptions();

                Log.Comment( $"URI: '{uri}" );

                CoAPUri.UriToComponents( uri, testSecureEndpoint, out scheme, out host, out port, out path, options );

                Log.Comment( $"SCHEME: {scheme}, HOST: '{host}', PORT: '{port}', PATH: {path}, OPTIONS: {options}" );
                Log.Comment( "" );
            }

            foreach(var uri in testSecureUris)
            {
                string scheme = null, host = null, path = null;
                int port = 0;
                var options = new MessageOptions();

                Log.Comment( $"URI: '{uri}" );

                CoAPUri.UriToComponents( uri, testSecureEndpointDifferent, out scheme, out host, out port, out path, options );

                Log.Comment( $"SCHEME: {scheme}, HOST: '{host}', PORT: '{port}', PATH: {path}, OPTIONS: {options}" );
                Log.Comment( "" );
            }

            Log.Comment( "" );
            Log.Comment( "" );
            Log.Comment( "Bad URIs" );
            Log.Comment( "" );


            foreach(var uri in testBadUris)
            {
                try
                {
                    string scheme = null, host = null, path = null;
                    int port = 0;
                    var options = new MessageOptions();
                    CoAPUri.UriToComponents( uri, testEndpoint, out scheme, out host, out port, out path, options );

                    Log.Comment( $"FAILURE: The following URI '{uri}' should have thrown exception" );

                    return TestResult.Fail;
                }
                catch(CoAP_UriFormatException)
                {

                }
            }

            Log.Comment( "*** COMPLETED: PASS" );
            Log.NewLine( );

            return TestResult.Pass;
        }
    }
}
