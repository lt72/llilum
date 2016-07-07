//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common.Diagnostics
{
    using System;
    using System.Runtime.CompilerServices;

    public class Logger : ILogger
    {
        private static ILogger s_logger = new Logger( );  

        //--//

        public static ILogger Instance
        {
            get
            {
                return s_logger;
            }
            set
            {
                s_logger = value;
            }
        }

        public virtual void Log( string msg ) { }
        public virtual void LogError( string msg ) { }
        public virtual void LogSuccess( string msg ) { }
        public virtual void LogWarning( string msg ) { }
    }
}
