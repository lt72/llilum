//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using LLOS = Zelig.LlilumOSAbstraction;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.Win32Product")]
    public sealed partial class ProcessorWin32 : RT.Processor
    {
        public enum IRQn_Type : int
        {
            //  Cortex-M Processor IRQ number, from cmsis implementation
            Reset_IRQn              = -15,      /*!< Exception#: 1 Reset (not actually defined as an IRQn_Type)   */
            NonMaskableInt_IRQn     = -14,      /*!< Exception#: 2 Non Maskable Interrupt                         */
            HardFault_IRQn          = -13,      /*!< Exception#: 3 Non Maskable Interrupt                         */
            MemoryManagement_IRQn   = -12,      /*!< Exception#: 4 Cortex-M3/4 Memory Management Interrupt        */
            BusFault_IRQn           = -11,      /*!< Exception#: 5 Cortex-M3/4 Bus Fault Interrupt                */
            UsageFault_IRQn         = -10,      /*!< Exception#: 6 Cortex-M3/4 Usage Fault Interrupt              */
            Reserved_IRQn9          = -9,       /*!< Exception#: 7 Reserved                                       */
            Reserved_IRQn8          = -8,       /*!< Exception#: 8 Reserved                                       */
            Reserved_IRQn7          = -7,       /*!< Exception#: 9 Reserved                                       */
            Reserved_IRQn6          = -6,       /*!< Exception#: 10 Reserved                                      */
            SVCall_IRQn             = -5,       /*!< Exception#: 11 Cortex-M3/4 SV Call Interrupt                 */
            DebugMonitor_IRQn       = -4,       /*!< Exception#: 12 Cortex-M3/4 Debug Monitor Interrupt           */
            Reserved_IRQn3          = -3,       /*!< Exception#: 13 Reserved                                      */
            PendSV_IRQn             = -2,       /*!< Exception#: 14 Cortex-M3/4 Pend SV Interrupt                 */
            SysTick_IRQn            = -1,       /*!< Exception#: 15 Cortex-M3/4 System Tick Interrupt             */
            //--//
            AnyInterrupt16           = 0,

            //--//

            Invalid = 0xFFFF,
        }

        public enum ISR_NUMBER : uint
        {
            //  Cortex-M Processor exception Numbers, as reported by the IPSR
            ThreadMode          = 0,
            Reset               = 1,
            NMI                 = 2,
            HardFault           = 3,
            MemManage           = 4,
            BusFault            = 5,
            UsageFault          = 6,
            Reserved7           = 7,
            Reserved8           = 8,
            Reserved9           = 9,
            Reserved10          = 10,
            SVCall              = 11,
            ReservedForDebug    = 12,
            Reserved13          = 13,
            PendSV              = 14,
            SysTick             = 15,
            //--//
            Peripheral          = 16,
            Last                = 240,
        }

        //
        // Exception priorities
        //

        public const uint c_Priority__MASK                  = 0x000000FFu;
        public const uint c_Priority__NeverDisabled         = 0x00000000u;
        public const uint c_Priority__Highest               = 0x00000001u;
        public const uint c_Priority__Lowest                = 0x000000FFu;
        public const uint c_Priority__HigherThanAnyWeOwn    = 0x00000004u;
        public const uint c_Priority__SVCCall               = 0x00000005u;
        public const uint c_Priority__Default               = 0x00000007u;
        public const uint c_Priority__SystemTimer           = c_Priority__Default;
        public const uint c_Priority__SysTick               = c_Priority__Default;
        public const uint c_Priority__PendSV                = 0x0000000Eu;

        //--//--//--//--//

        //
        // State
        //

        private static unsafe LLOS.HAL.TimerContext* s_timerForCompleteContextSwitch;
        
        //--//

        internal static uint SetBasePriRegister(uint basepri)
        {
            return LLOS.HAL.Interrupts.LLOS_INTERRUPTS_DisableInterruptsWithPriorityLevelLowerOrEqualTo(basepri);
        }

        public static uint DisableInterruptsWithPriorityLevelLowerOrEqualTo(uint basepri)
        {
            return SetBasePriRegister( basepri );
        }
        
        internal static int GetActiveIsrNumber()
        {
            return LLOS.HAL.Interrupts.LLOS_INTERRUPTS_GetActiveIsrNumber(); 
        }

        internal static void WaitForEvent()
        {
            LLOS.HAL.Interrupts.LLOS_INTERRUPTS_WaitForEvent(); 
        }

        internal static void InitiateContextSwitch( )
        {
            Peripherals.Instance.CauseInterrupt( ); 
        }

        static bool init = false;
        internal static void CompleteContextSwitch()
        {
            if(init == false)
            {
                unsafe
                {
                    fixed (LLOS.HAL.TimerContext** ppCtxSwitch = &s_timerForCompleteContextSwitch)
                    {
                        LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_AllocateTimer( Context.Emulated_PendSV_Handler_Zelig, ulong.MaxValue, ppCtxSwitch ), false );
                    }
                }

                init = true;
            }

            unsafe
            {
                fixed (LLOS.HAL.TimerContext** ppSwitch = &s_timerForCompleteContextSwitch)
                {
                    LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( s_timerForCompleteContextSwitch, 1 ), false );
                }
            }
        }
        
        [TS.WellKnownType( "Microsoft_Zelig_Win32_MethodWrapper" )]
        public sealed class MethodWrapper : RT.AbstractMethodWrapper
        {

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public override void Prologue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Prologue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  RT.HardwareException he )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public override void Epilogue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [RT.Inline]
            [RT.DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Epilogue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  RT.HardwareException he )
            {

            }

        }

        //
        // Access methods
        //

        public override RT.Processor.Context AllocateProcessorContext( RT.ThreadImpl owner )
        {
            return new Context( owner );
        }

        public override bool AreAllInterruptsDisabled( )
        {
            return AreInterruptsDisabled( );
        }

        public override bool AreInterruptsDisabled( )
        {
            bool fDisabled = LLOS.HAL.Interrupts.LLOS_INTERRUPTS_GetIsrPriorityLevel() <= c_Priority__Highest;

            if(fDisabled == false)
            {
                RT.BugCheck.Log( "DFUCK!!!!" );

                while(true) { }
            }

            return fDisabled;
        }

        public override bool AreInterruptsEnabled()
        {
            bool fEnabled = AreAllInterruptsDisabled() == false;

            if(fEnabled == false)
            {
                RT.BugCheck.Log( "EFUCK!!!!" ); 

                while(true) { }
            }

            return fEnabled;
        }

        //--//--//

        public static uint EnableInterrupts()
        {
            return DisableInterruptsWithPriorityLevelLowerOrEqualTo(c_Priority__Lowest);
        }

        public static uint DisableInterrupts()
        {
            return DisableInterruptsWithPriorityLevelLowerOrEqualTo(c_Priority__Highest);
        }

        //--//--//--//

        public override void Breakpoint( )
        {
            throw new NotImplementedException( );
        }

        public override void FlushCacheLine( UIntPtr target )
        {
            throw new NotImplementedException( );
        }

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            throw new NotImplementedException( );
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            throw new NotImplementedException( );
        }

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            RT.SmartHandles.InterruptState.DisableAll( );
        }
        
        [TS.GenerateUnsafeCast()]
        internal extern static RT.ThreadImpl CastAsThreadImpl(UIntPtr ptr);
        
        [DllImport("C")]
        public static extern void Breakpoint(uint value);

        [DllImport("C")]
        public static extern void Nop();
    }
}
