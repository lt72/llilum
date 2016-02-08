//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include <array>
#include <deque>

#include "LlosWin32.h"
#include <llos_thread.h>
#include <llos_system_timer.h>
#include <llos_interrupts.h>


//--//


//--//
//--//
//--//

uint32_t         g_ActiveInterruptNumber = 0;
uint32_t         g_InterruptsPriority    = 0xFF;
CRITICAL_SECTION g_InterruptsStateLock;

//--//
//--//
//--//

// TODO TODO: do not hardcode 255
NVIC_Emulation<255> g_EmulatedNVIC;

//--//

uint32_t LLOS_INTERRUPTS_GetActiveIsrNumber( )
{
    return g_ActiveInterruptNumber;
}

uint32_t LLOS_INTERRUPTS_GetIsrPriorityLevel( )
{
    wchar_t buffer[ 128 ];
    _snwprintf_s( buffer, 128, L"STATUSBASEPRI: %d", g_InterruptsPriority );
    LLOS_DEBUG_LogText( buffer, 128 );

    return g_InterruptsPriority;
}

uint32_t LLOS_INTERRUPTS_DisableInterruptsWithPriorityLevelLowerOrEqualTo( uint32_t basepri )
{
    //
    // Only acquire ownership if interrupts are not already disabled, in 
    // which case this thread owns the lock
    //
    EnterCriticalSection( &g_InterruptsStateLock );

    wchar_t buffer[ 128 ];
    _snwprintf_s( buffer, 128, L"ENTER BASEPRI: %d, DESIRED: %d", g_InterruptsPriority, basepri );
    LLOS_DEBUG_LogText( buffer, 128 );


    uint32_t currentPri = g_InterruptsPriority;

    if(currentPri <= c_Priority__Highest)
    {
        LeaveCriticalSection( &g_InterruptsStateLock );
    }

    g_InterruptsPriority = basepri;

    if(currentPri != basepri)
    {
        g_EmulatedNVIC.SignalPriorityLevelChange( currentPri, basepri );
    }

    //
    //  Only release ownership if this call does not disabled interrupts completely 
    //
    if(basepri > c_Priority__Highest)
    {
        LeaveCriticalSection( &g_InterruptsStateLock );
    }

    _snwprintf_s( buffer, 128, L"EXIT BASEPRI: %d, DESIRED: %d", g_InterruptsPriority, basepri );
    LLOS_DEBUG_LogText( buffer, 128 );

    return currentPri;
}

void LLOS_INTERRUPTS_WaitForEvent( )
{
    while(true)
    {

    }
}

void Nop( )
{
}

//--//

HRESULT LLOS_INTERRUPTS_Enable(int32_t irq)
{
    g_EmulatedNVIC.Enable(irq);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_Disable(int32_t irq)
{
    g_EmulatedNVIC.Disable(irq);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_SetPending(int32_t irq)
{
    if(irq == 200)
    {
        g_EmulatedNVIC.DispatchSysTick( );

        return S_OK;
    }
    
    g_EmulatedNVIC.SetPending(irq);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_ClearPending(int32_t irq)
{
    g_EmulatedNVIC.ClearPending(irq);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_GetActive(int32_t irq)
{
    g_EmulatedNVIC.GetActive(irq);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_SetPriority(int32_t irq, int32_t pri)
{
    g_EmulatedNVIC.SetPriority(irq, pri);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_GetPriority(int32_t irq, int32_t *pri)
{
    g_EmulatedNVIC.GetPriority(irq, pri);

    return S_OK;
}

HRESULT LLOS_INTERRUPTS_SetPriorityGrouping(uint32_t priority_group)
{
    g_EmulatedNVIC.SetPriorityGrouping(priority_group);

    return S_OK;
}

