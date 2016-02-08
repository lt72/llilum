#include "LlosWin32.h"
#include <llos_thread.h>
#include <llos_debug.h>

//--//

VOID WINAPI LlosThreadEntryWrapper(LPVOID lpThreadParameter)
{
    LlosThreadData *pThread = (LlosThreadData*)lpThreadParameter;

    try
    {
        pThread->entry(pThread->param);
    }
    catch (...)
    {
    }
}

HRESULT LLOS_THREAD_CreateThread(LLOS_ThreadEntry threadEntry, LLOS_Context threadParameter, LLOS_Context managedThreadCtx, uint32_t stackSize, LLOS_Handle* threadHandle)
{
    if (threadHandle == nullptr)
    {
        return E_INVALIDARG;
    }

    LlosThreadData *pFiberData = (LlosThreadData*)calloc(1, sizeof(LlosThreadData));

    if (pFiberData == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    pFiberData->entry            = threadEntry;
    pFiberData->param            = threadParameter;
    pFiberData->managedThreadCtx = managedThreadCtx;

    pFiberData->hndThread = CreateFiber( stackSize, LlosThreadEntryWrapper, pFiberData );

    if (pFiberData->hndThread == NULL)
    {
        free(pFiberData);
        pFiberData = nullptr;
    }

    *threadHandle = pFiberData;

    wchar_t buffer[ 128 ];
    _snwprintf_s( buffer, 128, L"Created Thread!  Context=%p, Param=0x%08x\r\n", pFiberData->managedThreadCtx, pFiberData->param );
    LLOS_DEBUG_LogText( buffer, 128 );

    return pFiberData != nullptr ? S_OK : E_FAIL;
}

HRESULT LLOS_THREAD_Start(LLOS_Handle threadHandle)
{
    //
    // Fiber do not need to get started
    //
    return S_OK;
}

HRESULT LLOS_THREAD_SwitchTo( LPVOID threadHandle )
{
    LlosThreadData *pThread = (LlosThreadData*)threadHandle;

    wchar_t buffer[ 128 ];
    _snwprintf_s( buffer, 128, L"SWITCH!  Context=%p, Param=0x%08x\r\n", pThread->managedThreadCtx, pThread->param );
    LLOS_DEBUG_LogText( buffer, 128 );

    ConvertThreadToFiber( NULL );

    SwitchToFiber(pThread->hndThread);

    ConvertFiberToThread( );

    return S_OK;
}

HRESULT LLOS_THREAD_SetPriority(LLOS_Handle threadHandle, LLOS_ThreadPriority threadPriority)
{
    int pri = THREAD_PRIORITY_NORMAL;
    LlosThreadData *pThread = (LlosThreadData*)threadHandle;

    if (pThread == nullptr)
    {
        return E_INVALIDARG;
    }

    switch (threadPriority)
    {
    case ThreadPriority_Lowest:
        pri = THREAD_PRIORITY_LOWEST;
        break;
    case ThreadPriority_BelowNormal:
        pri = THREAD_PRIORITY_BELOW_NORMAL;
        break;
    case ThreadPriority_Normal:
        pri = THREAD_PRIORITY_NORMAL;
        break;
    case ThreadPriority_AboveNormal:
        pri = THREAD_PRIORITY_ABOVE_NORMAL;
        break;
    case ThreadPriority_Highest:
        pri = THREAD_PRIORITY_HIGHEST;
        break;

    default:
        break;
    }

    return SetThreadPriority(pThread->hndThread, pri) ? S_OK : E_FAIL;
}

HRESULT LLOS_THREAD_GetPriority(LLOS_Handle threadHandle, LLOS_ThreadPriority* threadPriority)
{
    LLOS_ThreadPriority llosPri = ThreadPriority_Normal;
    LlosThreadData *pThread = (LlosThreadData*)threadHandle;

    if (pThread == nullptr || threadPriority == nullptr)
    {
        return E_INVALIDARG;
    }

    int pri = GetThreadPriority(pThread->hndThread);

    switch (pri)
    {
    case THREAD_PRIORITY_LOWEST:
        llosPri = ThreadPriority_Lowest;
        break;
    case THREAD_PRIORITY_BELOW_NORMAL:
        llosPri = ThreadPriority_BelowNormal;
        break;
    case THREAD_PRIORITY_NORMAL:
        llosPri = ThreadPriority_Normal;
        break;
    case THREAD_PRIORITY_ABOVE_NORMAL:
        llosPri = ThreadPriority_AboveNormal;
        break;
    case THREAD_PRIORITY_HIGHEST:
        llosPri = ThreadPriority_Highest;
        break;

    default:
        break;
    }

    *threadPriority = llosPri;

    return S_OK;
}

HRESULT LLOS_THREAD_DeleteThread(LLOS_Handle threadHandle)
{
    LlosThreadData *pThread = (LlosThreadData*)threadHandle;

    if (pThread != nullptr)
    {
        WaitForSingleObject(pThread->hndThread, -1);
        CloseHandle(pThread->hndThread);
        free(pThread);
    }

    return S_OK;
}

