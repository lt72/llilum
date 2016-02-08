//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public static class Thread
    {
        public enum ThreadPriority
        {
            Lowest      = 0,
            BelowNormal ,
            Normal      ,
            AboveNormal ,
            Highest     ,
        }

        public static unsafe uint LLOS_THREAD_CreateThread(Delegate dlgEntry, ThreadImpl threadImpl, ref UIntPtr threadHandle )
        {
            UIntPtr threadEntry;
            UIntPtr threadParam;
            UIntPtr managedThreadCtx;

            DelegateImpl dlg    = (DelegateImpl)(object)dlgEntry;
            threadEntry         = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            threadParam         = ( (ObjectImpl)dlgEntry.Target ).ToPointer( );
            managedThreadCtx    = ( (ObjectImpl)(object)threadImpl.SwappedOutContext).ToPointer( );

            return LLOS_THREAD_CreateThread( threadEntry, threadParam, managedThreadCtx, 8*1024, ref threadHandle );
        }

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_THREAD_CreateThread( UIntPtr threadEntry, UIntPtr threadParameter, UIntPtr managedThreadCtx, uint stackSize, ref UIntPtr threadHandle );
        
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_SwitchTo( UIntPtr threadHandle );
        
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_DeleteThread( UIntPtr threadHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_SetPriority( UIntPtr threadHandle, ThreadPriority threadPriority );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_GetPriority( UIntPtr threadHandle, out ThreadPriority threadPriority );
    }
}
