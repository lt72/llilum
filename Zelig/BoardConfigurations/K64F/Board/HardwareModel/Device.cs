//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F
{
    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.CortexM4OnMBED;

    public sealed class Device : ChipsetModel.Device
    {
        public override void PreInitializeProcessorAndMemory( )
        {
            //
            // Enter System mode, with interrupts disabled.
            //
            //
            // Enter System mode, with interrupts disabled.
            //
            Processor.SetStatusRegister( Processor.c_psr_field_c, Processor.c_psr_I | Processor.c_psr_F | Processor.c_psr_mode_SYS );

            Processor.SetRegister( Processor.Context.RegistersOnStack.StackRegister, this.BootstrapStackPointer );
        }

        [RT.MemoryUsage(RT.MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true)]
        static readonly uint[] s_bootstrapStackK64F = new uint[ 1024 / sizeof( uint ) ]; 

        //
        // Access Methods
        //

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackK64F;
            }
        }

        public override void MoveCodeToProperLocation( )
        {
            ChipsetModel.Memory.Instance.ExecuteImageRelocation( );
        }
    }
}
