//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnCMSISCore
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM0;
    using LLOS         = Zelig.LlilumOSAbstraction.HAL;

    public abstract class Processor : ChipsetModel.Processor
    {
        public abstract new class Context : ChipsetModel.Processor.Context
        {
            //
            // Constructor Methods
            //

            //
            // Helper Methods
            //

            //
            // Access Methods
            //
        }
        

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();
        }
    }
    
    //--//
    //--//
    //--//

    [RT.ExtendClass( typeof( Microsoft.Zelig.Runtime.Processor ) )]
    internal class ProcessorImpl
    {
        [RT.MergeWithTargetImplementation]
        internal ProcessorImpl()
        {
        }

        [RT.NoInline]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        public static int Delay( int count )
        {
            LLOS.Clock.LLOS_CLOCK_DelayCycles( (uint)count );
            return 0;
        }
    }
}
