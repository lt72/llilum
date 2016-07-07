using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoAP.Common; 

namespace CoAP.Stack.Abstractions
{
    public interface IEncode
    {
        byte[ ] Encode( byte[ ] buffer, int offset ); 
    }
}
