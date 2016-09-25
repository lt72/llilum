//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM3OnCMSISCore
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM3;
    using CortexM      =  Microsoft.DeviceModels.Chipset.CortexM;
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
            
            //
            // Reset the priority grouping that we assume not used
            //
            CortexM.NVIC.SetPriorityGrouping( 0 );
        }

        protected override unsafe void RemapInterrupt(IRQn_Type IRQn, Action isr)
        {
            RT.DelegateImpl dlg = (RT.DelegateImpl)(object)isr;

            UIntPtr isrPtr = new UIntPtr(dlg.InnerGetCodePointer().Target.ToPointer());

            CortexM.NVIC.SetVector((int)IRQn, isrPtr.ToUInt32());
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
