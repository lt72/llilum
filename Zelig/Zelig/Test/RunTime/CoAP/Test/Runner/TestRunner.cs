//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SPOT.Platform.Tests;


    public class TestRunner
    {
        private int m_timeOut = 900000; // 15 minutes default time out.

        /// <summary>
        /// An overloaded constructor that takes the test objects in its arguments.
        /// </summary>
        /// <param name="args">A list of test objects.</param>
        public TestRunner(int timeout)
        {
            m_timeOut = timeout; 
        }

        public void Run()
        {
            var tests = new List<TestBase>();
            bool testResults = true;

            tests.Add( new TestUris                 ( ) );
            tests.Add( new TestPiggyBackedResponse  ( ) );
            tests.Add( new TestDelayedResponse      ( ) );
            tests.Add( new TestNonIdempotentMethods ( ) );
            tests.Add( new TestMultiClient          ( ) );
            tests.Add( new TestUnsupportedMethods   ( ) );
            tests.Add( new TestServerReset          ( ) );
            tests.Add( new TestBadOptions           ( ) );
            tests.Add( new TestContentFormat        ( ) );

            foreach(ITestInterface t in tests)
            {
                try
                {
                    var test = (TestBase)t;
                    TestConsole.WriteLine( $"++++++++++++++++++++++++++++++++++++++++++++++++" );
                    TestConsole.WriteLine( $"+++++ TEST '{test.Name}' running..." );
                    TestConsole.WriteLine( $"++++++++++++++++++++++++++++++++++++++++++++++++" );

                    if(t.Initialize( ) == InitializeResult.ReadyToGo)
                    {
                        TestResult result = test.Run(null);

                        string resultString = "Passed";
                        if((result & TestResult.Fail) != 0)
                        {
                            resultString = "Failed";
                            testResults = false;
                        }

                        TestConsole.WriteLine( "Result: " + resultString );
                    }
                }
                catch
                {
                    TestConsole.WriteLine( "######## Caught exception while running tests ########" );
                    TestConsole.WriteLine( "######## Caught exception while running tests ########" );
                    TestConsole.WriteLine( "######## Caught exception while running tests ########" );

                    testResults = false;
                }
                finally
                {
                    t.CleanUp( );
                    TestConsole.WriteLine( "------------------------------------------------" );
                    TestConsole.WriteLine( "------------------------------------------------" );
                    TestConsole.WriteLine( "" );
                }
            }

            TestConsole.WriteLine("All tests complete.");
            TestConsole.WriteLine("");
            TestConsole.WriteLine( "++++++++++++++++++++++++++++++++++++++++++++++++" );
            TestConsole.WriteLine( "+++++ Test Run Result: " + ( testResults ? "PASS" : "FAIL" ));
            TestConsole.WriteLine( "++++++++++++++++++++++++++++++++++++++++++++++++" );
        }

        public static void Main(string[] args)
        {
            new TestRunner(0).Run();

            TestConsole.WriteLine( "Test completed, press any key to exit" ); 
            Console.Read( ); 
        }
    }
}
