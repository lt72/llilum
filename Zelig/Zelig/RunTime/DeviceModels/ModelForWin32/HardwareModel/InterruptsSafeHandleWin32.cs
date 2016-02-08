//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.Win32.SmartHandles
{
    using System;

    using LLOS      = LlilumOSAbstraction;
    using ISA       = Microsoft.Zelig.TargetModel.Win32.InstructionSetVersion;
    using Chipset   = Microsoft.DeviceModels.Win32;

    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState), PlatformFamilyFilter = ISA.Platform_Family__Win32 )]
    public struct InterruptStateWin32 : IDisposable
    {
        //
        // State
        //

        uint m_basepri;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptStateWin32(uint basepri)
        {
            m_basepri = basepri;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            Chipset.ProcessorWin32.DisableInterruptsWithPriorityLevelLowerOrEqualTo(m_basepri);
        }

        [Inline]
        public void Toggle()
        {
            uint basepri = Chipset.ProcessorWin32.SetBasePriRegister(m_basepri);
            Chipset.ProcessorWin32.Nop();
            Chipset.ProcessorWin32.CompleteContextSwitch( ); 

            Chipset.ProcessorWin32.SetBasePriRegister(basepri);
        }

        //--//

        [Inline]
        public static InterruptStateWin32 Disable()
        {
            return new InterruptStateWin32(Chipset.ProcessorWin32.DisableInterrupts());
        }

        [Inline]
        public static InterruptStateWin32 DisableAll()
        {
            return new InterruptStateWin32(Chipset.ProcessorWin32.DisableInterrupts());
        }

        [Inline]
        public static InterruptStateWin32 Enable()
        {
            return new InterruptStateWin32(Chipset.ProcessorWin32.EnableInterrupts());
        }

        [Inline]
        public static InterruptStateWin32 EnableAll()
        {
            return new InterruptStateWin32(Chipset.ProcessorWin32.EnableInterrupts());
        }

        //
        // Access Methods
        //

        [Inline]
        public uint GetPreviousState()
        {
            return m_basepri;
        }

        public HardwareException GetCurrentExceptionMode()
        {
            Chipset.ProcessorWin32.ISR_NUMBER ex = GetMode();

            if (ex == Chipset.ProcessorWin32.ISR_NUMBER.ThreadMode)
            {
                return HardwareException.None;
            }

            switch (ex)
            {
                case Chipset.ProcessorWin32.ISR_NUMBER.NMI: return HardwareException.NMI;
                case Chipset.ProcessorWin32.ISR_NUMBER.HardFault: return HardwareException.Fault;
                case Chipset.ProcessorWin32.ISR_NUMBER.MemManage: return HardwareException.Fault;
                case Chipset.ProcessorWin32.ISR_NUMBER.BusFault: return HardwareException.Fault;
                case Chipset.ProcessorWin32.ISR_NUMBER.UsageFault: return HardwareException.Fault;
                case Chipset.ProcessorWin32.ISR_NUMBER.SVCall: return HardwareException.Service;
                case Chipset.ProcessorWin32.ISR_NUMBER.ReservedForDebug: return HardwareException.Debug;
                case Chipset.ProcessorWin32.ISR_NUMBER.PendSV: return HardwareException.SoftwareInterrupt;
                case Chipset.ProcessorWin32.ISR_NUMBER.SysTick: return HardwareException.SoftwareInterrupt;
                case Chipset.ProcessorWin32.ISR_NUMBER.Reset:
                case Chipset.ProcessorWin32.ISR_NUMBER.Reserved7:
                case Chipset.ProcessorWin32.ISR_NUMBER.Reserved8:
                case Chipset.ProcessorWin32.ISR_NUMBER.Reserved9:
                case Chipset.ProcessorWin32.ISR_NUMBER.Reserved10:
                case Chipset.ProcessorWin32.ISR_NUMBER.Reserved13: BugCheck.Assert(false, BugCheck.StopCode.IllegalMode); break;

                default: return HardwareException.Interrupt;
            }

            return HardwareException.Interrupt;
        }

        //--//

        private Chipset.ProcessorWin32.ISR_NUMBER GetMode()
        {
            return (Chipset.ProcessorWin32.ISR_NUMBER)(Chipset.ProcessorWin32.GetActiveIsrNumber() & 0x1FF);
        }
    }
}
