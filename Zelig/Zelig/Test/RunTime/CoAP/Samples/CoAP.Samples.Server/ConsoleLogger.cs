//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Samples.Server
{
    using System;
    using System.Runtime.CompilerServices;
    using CoAP.Common.Diagnostics;

    public class ConsoleLogger : Logger
    {
        private ConsoleColor m_previousColor = Console.ForegroundColor;

        //--//

        [MethodImpl( MethodImplOptions.Synchronized )]
        public override void Log( string msg )
        {
            var now = DateTime.Now;

            var timestamp = $" @{ now.Minute}m::{ now.Second}s::{ now.Millisecond}ms";

            Console.WriteLine( msg + timestamp );
        }

        [MethodImpl( MethodImplOptions.Synchronized )]
        public override void LogSuccess( string msg )
        {
            Color = ConsoleColor.Green;

            Log( msg ); 

            ResetColor( );
        }

        [MethodImpl( MethodImplOptions.Synchronized )]
        public override void LogWarning( string msg )
        {
            Color = ConsoleColor.Yellow;

            Log( msg );

            ResetColor( );
        }

        [MethodImpl( MethodImplOptions.Synchronized )]
        public override void LogProtocolError( string msg )
        {
            Color = ConsoleColor.Magenta;

            Log( msg );

            ResetColor( );
        }

        [MethodImpl( MethodImplOptions.Synchronized )]
        public override void LogError( string msg )
        {
            Color = ConsoleColor.Red;

            Log( msg );

            ResetColor( );
        }

        protected ConsoleColor Color
        {
            get
            {
                return Console.ForegroundColor;
            }
            set
            {
                m_previousColor = Console.ForegroundColor;

                Console.ForegroundColor = value;
            }
        }

        protected ConsoleColor ResetColor()
        {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = m_previousColor; 

            return color; 
        }
    }
}
