//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Runtime.Win32
{
    using System;

    using RT      = Microsoft.Zelig.Runtime;
    using LLOS    = Zelig.LlilumOSAbstraction;
    using Chipset = Microsoft.DeviceModels.Win32;


    public sealed class Win32ThreadManager : RT.ThreadManager
    {
        public const uint c_TimeQuantumMsec = 20;

        //
        // State
        //

        //
        // BUGBUG: we need to Dispose this object on shutdown !!!
        //
        private unsafe LLOS.HAL.TimerContext* m_timerForWaits;
        private unsafe LLOS.HAL.TimerContext* m_contextSwitchTimer;

        //
        // Helper Methods
        //

        public override void InitializeBeforeStaticConstructors()
        {
            base.InitializeBeforeStaticConstructors();
        }

        public override void InitializeAfterStaticConstructors(uint[] systemStack)
        {
            base.InitializeAfterStaticConstructors(systemStack);
        }
        
        public override unsafe void StartThreads( )
        {
            //
            // The standard thread manager will set the current thread to be the 
            // idle thread before letting the scheduler pick up the next target.
            // that means that the Context Switch code will find a current thread and 
            // will try to update the stack pointer of its context to the psp value on
            // the processor. We need to initialize the PSP value to whatever
            // we want the context switch to persist in the current (i.e. idle) thread
            // context. As for the general case, the Idle Thread context stack pointer 
            // is initialized to be the end of the first frame, which though really never 
            // ran. So we will initialized the actual psp register to the base of the 
            // Idle Thread stack pointer at this stage.
            //

            RT.BugCheck.Log( "================= STARTING THREADS" );

            //
            // Enable context switch through SVC call that will fall back into Thread/PSP mode onto 
            // whatever thread the standard thread manager intended to switch into 
            //
            DeviceModels.Win32.ProcessorWin32.DisableInterruptsWithPriorityLevelLowerOrEqualTo( DeviceModels.Win32.ProcessorWin32.c_Priority__SVCCall + 1 );

            //
            // Let the standard thread manager set up the next thread to run and request the switch to its context
            // It will be a switch to the idle thread (bootstrap thread)
            //
            RT.BugCheck.Log( "================= CALL BASE..." );
            base.StartThreads( );
            RT.BugCheck.Log( "================= ...DONE CALL BASE" );

            //
            // Never come back from this!
            //

            RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!" );
            RT.BugCheck.Log( "!!! Back in Thread Manager, Ctx Switch Failed !!!" );
            RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!" );

            RT.BugCheck.Assert( false, RT.BugCheck.StopCode.CtxSwitchFailed );
        }
        
        public override void RemoveThread( RT.ThreadImpl thread )
        {
            //
            // This should schedule a context switch
            //
            base.RemoveThread( thread ); 

            //
            // stage an emulated PendSV request to complete the ContextSwitch
            //
            DeviceModels.Win32.ProcessorWin32.CompleteContextSwitch();

            //
            // We should never get here
            //
            RT.BugCheck.Assert( false, RT.BugCheck.StopCode.CtxSwitchFailed ); 
        }
        
        private void WaitExpired(ulong time)
        {

            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                WaitExpired( RT.SchedulerTime.FromUnits( time ) );
            }
        }

        private void TimeQuantumExpired(ulong time)
        {
            TimeQuantumExpired( ); 
        }

        public override void Activate()
        {
            base.Activate();

            unsafe
            {
                fixed (LLOS.HAL.TimerContext** ppWait = &m_timerForWaits)
                {
                    LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_AllocateTimer( WaitExpired, ulong.MaxValue, ppWait ), false );
                }
            }
            
            unsafe
            {
                fixed (LLOS.HAL.TimerContext** ppSwitch = &m_contextSwitchTimer)
                {
                    LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_AllocateTimer( TimeQuantumExpired, ulong.MaxValue, ppSwitch ), false );
                }
            }

            //DeviceModels.Win32.InterruptController.Instance.Activate();
        }

        public override void CancelQuantumTimer()
        {
            unsafe
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_contextSwitchTimer, ulong.MaxValue ), false );
            }
        }
        
        public override bool ShouldContextSwitch
        {
            [RT.Inline]
            get
            {
                return m_runningThread != m_nextThread;
            }
        }

        public override void SetNextQuantumTimer()
        {
            unsafe
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_contextSwitchTimer, c_TimeQuantumMsec * 1000 ), false );
            }
        }

        public override void SetNextQuantumTimer(RT.SchedulerTime nextTimeout)
        {
            //////DateTime dt = (DateTime)nextTimeout;
            //////const long TicksPerMillisecond = 10000; // Number of 100ns ticks per time unit
            //////long ms = dt.Ticks / TicksPerMillisecond;

            //////if (ms > Chipset.Drivers.ContextSwitchTimer.c_MaxCounterValue)
            //////{
            //////    RT.BugCheck.Assert(false, RT.BugCheck.StopCode.IllegalSchedule);
            //////}

            ulong qt = (ulong)nextTimeout.Units / 10;
            
            RT.BugCheck.Log( "TM ==> SetNextQuantumTimer=0x%08x", (int)qt );

            unsafe
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_contextSwitchTimer, qt ), false );
            }
        }

        public override void TimeQuantumExpired()
        {
            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                //
                // this will cause the reschedule
                //
                base.TimeQuantumExpired( );

                //
                // stage an emulated PendSV request to complete the ContextSwitch
                //
                DeviceModels.Win32.ProcessorWin32.CompleteContextSwitch( );
            }
        }

        protected override void IdleThread()
        {
            RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
            RT.BugCheck.Log( "!!! Idle thread running !!!" );
            RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );

             //DeviceModels.Win32.ProcessorWin32.InitiateContextSwitch( );
             
            unsafe
            {
                LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_contextSwitchTimer, 1 ), false );
            }

            RT.SmartHandles.InterruptState.EnableAll( ); 
             
            while(true)
            {
                RT.ThreadImpl currentThread = this.CurrentThread;
                RT.ThreadImpl nextThread    = this.NextThread;
                        
                //
                // Update thread manager state and Thread.CurrentThread static field
                //
                this.CurrentThread = nextThread;

                RT.ThreadImpl.CurrentThread = nextThread;

                if(nextThread == m_idleThread)
                {
                    RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
                    RT.BugCheck.Log( "!!!       sleeping      !!!" );
                    RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );

                    DeviceModels.Win32.ProcessorWin32.WaitForEvent( );
                }
                else
                {
                    RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
                    RT.BugCheck.Log( "!!!      switching      !!!" );
                    RT.BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!" );

                    nextThread.SwappedOutContext.SwitchTo( ); 
                }
            }
        }

        public override void SetNextWaitTimer(RT.SchedulerTime nextTimeout)
        {
            if (nextTimeout != RT.SchedulerTime.MaxValue)
            {
                unsafe
                {
                    LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_timerForWaits, nextTimeout.Units / 10 ), false );
                }
            }
            else
            {
                unsafe
                {
                    LLOS.LlilumErrors.ThrowOnError( LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_timerForWaits, ulong.MaxValue ), false );
                }
            }
        }

        //
        // Access methods
        //
        
        public RT.ThreadImpl SwitcherThread
        {
            get
            {
                return m_idleThread;
            }
        }
        public override RT.ThreadImpl InterruptThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RT.ThreadImpl FastInterruptThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RT.ThreadImpl AbortThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //--//

    }
}
