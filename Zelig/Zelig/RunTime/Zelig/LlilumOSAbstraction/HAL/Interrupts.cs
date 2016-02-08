//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public static class Interrupts
    {

        internal static int GetActiveIsrNumber()
        {
            return LLOS_INTERRUPTS_GetActiveIsrNumber(); 
        }
        
        internal static uint SetBasePriRegister(uint basepri)
        {
            return LLOS_INTERRUPTS_DisableInterruptsWithPriorityLevelLowerOrEqualTo(basepri);
        }

        public static uint DisableInterruptsWithPriorityLevelLowerOrEqualTo(uint basepri)
        {
            return SetBasePriRegister( basepri );
        }

        //--//

        [DllImport("C")]
        public static extern int LLOS_INTERRUPTS_GetActiveIsrNumber();

        [DllImport("C")]
        public static extern uint LLOS_INTERRUPTS_DisableInterruptsWithPriorityLevelLowerOrEqualTo(uint basepri);

        [DllImport("C")]
        public static extern uint LLOS_INTERRUPTS_GetIsrPriorityLevel();

        [DllImport( "C" )]
        public static extern void LLOS_INTERRUPTS_WaitForEvent( );
    }
}
