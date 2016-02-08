//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.DeviceModels.Win32
{
    using System;

    using RT        = Microsoft.Zelig.Runtime;
    using LLOS      = Zelig.LlilumOSAbstraction.HAL;
    using Chipset   = Microsoft.DeviceModels.Win32;

    public class Peripherals : RT.Peripherals
    {

        //
        // State
        //

        //
        // Helper Methods
        //

        public override void Initialize()
        {
            RT.BugCheck.AssertInterruptsOff();

            //
            // Faults, never disabled, emulating a Cortex-M3...
            //
            //////CMSIS.NVIC.SetPriority( (int)ProcessorARMv7M.IRQn_Type.HardFault_IRQn       , ProcessorARMv7M.c_Priority__NeverDisabled );
            //////CMSIS.NVIC.SetPriority( (int)ProcessorARMv7M.IRQn_Type.MemoryManagement_IRQn, ProcessorARMv7M.c_Priority__NeverDisabled );
            //////CMSIS.NVIC.SetPriority( (int)ProcessorARMv7M.IRQn_Type.BusFault_IRQn        , ProcessorARMv7M.c_Priority__NeverDisabled );
            //////CMSIS.NVIC.SetPriority( (int)ProcessorARMv7M.IRQn_Type.UsageFault_IRQn      , ProcessorARMv7M.c_Priority__NeverDisabled );

            //
            // System exceptions, emulating a Cortex-M3...
            //
            Chipset.NVIC.SetPriority( (int)Chipset.ProcessorWin32.IRQn_Type.SVCall_IRQn , Chipset.ProcessorWin32.c_Priority__SVCCall );
            Chipset.NVIC.SetPriority( (int)Chipset.ProcessorWin32.IRQn_Type.SysTick_IRQn, Chipset.ProcessorWin32.c_Priority__SysTick );
            Chipset.NVIC.SetPriority( (int)Chipset.ProcessorWin32.IRQn_Type.PendSV_IRQn , Chipset.ProcessorWin32.c_Priority__PendSV  );
        }
        
        public override void Activate()
        {
        }

        public override void EnableInterrupt( uint index )
        {
            throw new NotImplementedException( );
        }

        public override void DisableInterrupt( uint index )
        {
            throw new NotImplementedException( );
        }

        public override void CauseInterrupt()
        {
            InterruptController.Instance.CauseInterrupt( ); 
        }

        public override void ContinueUnderNormalInterrupt(Continuation dlg)
        {
            throw new NotImplementedException( );
        }

        public override void WaitForInterrupt()
        {
            while (true)
            {
                Chipset.ProcessorWin32.WaitForEvent();
            }
        }

        public override void ProcessInterrupt()
        {
            throw new NotImplementedException( );
        }

        [RT.MemoryRequirements(RT.MemoryAttributes.RAM)]
        public override void ProcessFastInterrupt()
        {
            throw new NotImplementedException( );
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return LLOS.Clock.LLOS_CLOCK_GetPerformanceCounterFrequency();
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        public override uint ReadPerformanceCounter()
        {
            return (uint)LLOS.Clock.LLOS_CLOCK_GetPerformanceCounter();
        }
    }
}
