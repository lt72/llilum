﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM4OnMBED
{
    using System;
    
    using RT            = Microsoft.Zelig.Runtime;
    using RTOS          = Microsoft.Zelig.Support.mbed;
    using ChipsetModel  = Microsoft.CortexM4OnCMSISCore;
    

    public abstract class Processor : ChipsetModel.Processor 
    {

        public new abstract class Context : ChipsetModel.Processor.Context
        {   
        }

        //--//
        
        //
        // Access methods
        //
        
        public static ulong CoreClockFrequency
        {
            [RT.ConfigurationOption("System__CoreClockFrequency")]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 0;
            }
        }

        public static ulong RealTimeClockFrequency
        {
            [RT.ConfigurationOption("System__RealTimeClockFrequency")]
            get
            { 
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 0;
            }
        }
            
        public static uint DefaultThreadPoolThreads
        {
            [RT.ConfigurationOption( "System__Runtime_DefaultThreadPoolThreads" )]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 3;
            }
        }

        public static uint DefaultTimerPooThreads
        {
            [RT.ConfigurationOption( "System__Runtime_DefaultTimerPooThreads" )]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 2;
            }
        }


        //--//

        [RT.BottomOfCallStack( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        private static void InterruptHandler( ref Context.RegistersOnStack stackPtr )
        {
            s_repeatedAbort = false;
            Context.InterruptHandlerWithContextSwitch( ref stackPtr );
        }

        [RT.BottomOfCallStack( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.FastInterrupt )]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        private static void FastInterruptHandler( )
        {
            s_repeatedAbort = false;
            Context.FastInterruptHandlerWithoutContextSwitch( );
        }

        [RT.BottomOfCallStack( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.SoftwareInterrupt )]
        private static void SoftwareInterruptHandler( ref Context.RegistersOnStack registers )
        {
            s_repeatedAbort = false;
            Context.GenericSoftwareInterruptHandler( ref registers );
        }

        //--//

        [RT.NoInline]
        [RT.NoReturn( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.UndefinedInstruction )]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        static void UndefinedInstruction( )
        {
            RT.Processor.Instance.Breakpoint( );
        }

        [RT.NoInline]
        [RT.NoReturn( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.PrefetchAbort )]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        static void PrefetchAbort( )
        {
            RT.Processor.Instance.Breakpoint( );
        }


        private static bool s_repeatedAbort = false;
        private static int s_abortCount = 0;

        [RT.NoInline]
        [RT.NoReturn( )]
        [RT.HardwareExceptionHandler( RT.HardwareException.DataAbort )]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        static void DataAbort( )
        {
            bool repeatedAbort = s_repeatedAbort;
            s_repeatedAbort = true;
            s_abortCount++;
            if(repeatedAbort)
                RT.Processor.Instance.Breakpoint( );
        }
    }
}
