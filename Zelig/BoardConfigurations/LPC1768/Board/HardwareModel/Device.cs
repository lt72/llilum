//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.CortexM3OnMBED;

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

        // TODO: When the compiler optimizations are complete, revisit this stack size since it is likely
        // it could be reduced.
        [RT.MemoryUsage(RT.MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true)]
        static readonly uint[] s_bootstrapStackLPC1768 = new uint[ 512 / sizeof( uint ) ];

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackLPC1768;
            }
        }

        public override void MoveCodeToProperLocation( )
        {
            ChipsetModel.Memory.Instance.ExecuteImageRelocation( );
        }
    }
}
