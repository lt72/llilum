using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoAP.Common.Diagnostics
{
    public interface ILogger
    {
        void Log( string msg );

        void LogSuccess( string msg );

        void LogWarning( string msg );

        void LogProtocolError( string msg );

        void LogError( string msg );
    }
}
