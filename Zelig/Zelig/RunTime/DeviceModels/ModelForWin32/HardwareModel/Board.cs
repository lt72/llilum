//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Win32
{
    using System.Runtime.CompilerServices;
    
    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Win32;

    
    public class Board 
    {
        //
        // System timer
        //
        public virtual int GetSystemTimerIRQNumber( )
        {
            return 200;
        }
                        
        //
        // Factory methods
        //

        public static extern Board Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}

