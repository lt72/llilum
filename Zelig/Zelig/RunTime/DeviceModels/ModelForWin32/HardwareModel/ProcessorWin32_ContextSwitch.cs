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
    public partial class ProcessorWin32 : RT.Processor
    {

        public new class Context : RT.Processor.Context
        {
            private UIntPtr m_threadHandle;

            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            public override UIntPtr BaseStackPointer
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public override uint ExcReturn
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr ProgramCounter
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override uint ScratchedIntegerRegisters
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr StackPointer
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public override UIntPtr GetRegisterByIndex( uint idx )
            {
                throw new NotImplementedException( );
            }

            public override void Populate( )
            {
                throw new NotImplementedException( );
            }

            public override void Populate( RT.Processor.Context context )
            {
                throw new NotImplementedException( );
            }

            public override void PopulateFromDelegate( Delegate dlg, uint[] stack )
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_CreateThread(dlg, m_owner, ref m_threadHandle), false);
            }

            public override void SetRegisterByIndex( uint idx, UIntPtr value )
            {
                throw new NotImplementedException( );
            }

            public override void SetupForExceptionHandling( uint mode )
            {
                throw new NotImplementedException( );
            }

            public override void SwitchTo( )
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_SwitchTo( this.m_threadHandle ), false);
            }

            public override bool Unwind( )
            {
                throw new NotImplementedException( );
            }

            public void Retire()
            {
                LLOS.HAL.Thread.LLOS_THREAD_DeleteThread(m_threadHandle);
            }
            
            private static void ContextSwitch( RT.ThreadManager tm )
            {
                RT.ThreadImpl currentThread = tm.CurrentThread;
                RT.ThreadImpl nextThread    = tm.NextThread;
                        
                //
                // Update thread manager state and Thread.CurrentThread static field
                //
                tm.CurrentThread = nextThread;

                RT.ThreadImpl.CurrentThread = nextThread;

                //LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Thread.LLOS_THREAD_SwitchTo(((RT.ObjectImpl)(object)nextThread).ToPointer()), false);
                nextThread.SwappedOutContext.SwitchTo( ); 
            }
            
            [RT.CapabilitiesFilter( RequiredCapabilities=Microsoft.Zelig.TargetModel.Win32.InstructionSetVersion.Platform_Family__Win32 )]
            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            internal static void Emulated_PendSV_Handler_Zelig( ulong t )
            {
                using(RT.TargetPlatform.Win32.SmartHandles.InterruptStateWin32.Disable( ))
                {
                    unsafe
                    {
                        ContextSwitch( RT.ThreadManager.Instance );
                    }
                }
            }

            
            //////private static void ContextSwitch( RT.ThreadManager tm )
            //////{
            //////    //
            //////    // Use the Idle Thread to switch to the thread we neeed
            //////    //
            //////    ((Runtime.Win32.Win32ThreadManager)tm).SwitcherThread.SwappedOutContext.SwitchTo( ); 
            //////}
            
            //////[RT.CapabilitiesFilter( RequiredCapabilities=Microsoft.Zelig.TargetModel.Win32.InstructionSetVersion.Platform_Family__Win32 )]
            //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            //////[RT.ExportedMethod]
            //////internal static void Emulated_PendSV_Handler_Zelig( ulong t )
            //////{
            //////    using(RT.TargetPlatform.Win32.SmartHandles.InterruptStateWin32.Disable( ))
            //////    {
            //////        unsafe
            //////        {
            //////            ContextSwitch( RT.ThreadManager.Instance );
            //////        }
            //////    }
            //////}
        }
    }
}
