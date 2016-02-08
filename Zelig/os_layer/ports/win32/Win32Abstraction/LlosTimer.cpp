#include "LlosWin32.h"
#include <llos_mutex.h>
#include <llos_interrupts.h>
#include <llos_system_timer.h>
#include <llos_debug.h>


//--//
//--//

SystemTimer_Emulation g_EmulatedSystemTimer;

//--//

HRESULT LLOS_SYSTEM_TIMER_AllocateTimer(LLOS_SYSTEM_TIMER_Callback callback, LLOS_Context callbackContext, uint64_t microsecondsFromNow, LLOS_Context *pTimer)
{
    static bool initialized = false;

    if(initialized == false)
    {
        g_EmulatedSystemTimer.Initialize( ); 

        initialized = true;
    }

    LlosTimerEntry *pEntry = nullptr;

    if (pTimer == nullptr || callback == nullptr)
    {
        return E_INVALIDARG;
    }

    pEntry = (LlosTimerEntry*)calloc(1, sizeof(LlosTimerEntry));

    if (pEntry != nullptr)
    {
        pEntry->callback        = callback;
        pEntry->callbackContext = callbackContext;
        pEntry->fCancelled      = FALSE;
        pEntry->waitTime        = microsecondsFromNow; 
        pEntry->expiredTime     = 0ll;
        
        pEntry->hndEvent  = CreateEvent ( NULL, FALSE, FALSE, NULL );
        pEntry->hndThread = CreateThread( NULL, 1024 * 1024, &SystemTimer_Emulation::TimerWaitProcedure, pEntry, 0, NULL );

        SetThreadPriority( pEntry->hndThread, THREAD_PRIORITY_HIGHEST );

        pEntry->timer     = &g_EmulatedSystemTimer;

        *pTimer = pEntry;
        
        g_EmulatedSystemTimer.RegisterTimer( pEntry );

        wchar_t buffer[ 128 ];
        _snwprintf_s( buffer, 128, L"Created timer: %p, timeout: %lld\r\n", pEntry, microsecondsFromNow );
        LLOS_DEBUG_LogText( buffer, 128 );
    }

    return pEntry != nullptr ? S_OK : E_FAIL;
}

VOID LLOS_SYSTEM_TIMER_FreeTimer(LLOS_Context pTimer)
{
    if (pTimer != NULL)
    {
        LlosTimerEntry *pEntry = (LlosTimerEntry*)pTimer;

        g_EmulatedSystemTimer.DeregisterTimer( pEntry );

        pEntry->fCancelled = TRUE;
        SetEvent(pEntry->hndEvent);
        WaitForSingleObject(pEntry->hndThread, INFINITE);
        CloseHandle(pEntry->hndThread);
        CloseHandle(pEntry->hndEvent);
        free(pTimer);
    }
}

HRESULT LLOS_SYSTEM_TIMER_ScheduleTimer(LLOS_Context pTimer, uint64_t microsecondsFromNow)
{
    LlosTimerEntry *pEntry = (LlosTimerEntry*)pTimer;

    if (pTimer == nullptr)
    {
        return E_INVALIDARG;
    }

    wchar_t buffer[ 128 ];
    _snwprintf_s( buffer, 128, L"Schedule timer: %p, timeout: %lld\r\n", pTimer, microsecondsFromNow );
    LLOS_DEBUG_LogText( buffer, 128 );

    pEntry->waitTime = microsecondsFromNow;

    SetEvent(pEntry->hndEvent);

    return S_OK;
}

uint64_t LLOS_SYSTEM_TIMER_GetTicks(LLOS_Context pTimer)
{
    return GetTickCount64();
}

uint64_t LLOS_SYSTEM_TIMER_GetTimerFrequency(LLOS_Context pTimer)
{
    return 1000;
}

