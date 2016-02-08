#include <Windows.h>
#include <stdio.h>
#include <tchar.h>
#include <array>
#include <deque>
#include <set>


#include <llos_types.h>
#include <llos_debug.h>
#include <llos_interrupts.h>
#include <llos_system_timer.h>


#ifndef __LLOS_WIN32_H__
#define __LLOS_WIN32_H__

extern uint32_t         g_InterruptsPriority;

typedef struct LlosThreadData
{
    LLOS_ThreadEntry entry;
    LLOS_Context     param;
    LLOS_Context     managedThreadCtx;
    HANDLE           hndThread;
} LlosThread;

class SystemTimer_Emulation;

typedef struct LlosTimerEntry
{
    HANDLE                      hndEvent;
    HANDLE                      hndThread;
    LLOS_SYSTEM_TIMER_Callback  callback;
    LLOS_Context                callbackContext;
    uint64_t                    waitTime;
    uint64_t                    expiredTime;
    BOOL                        fCancelled;
    SystemTimer_Emulation*      timer;
} LlosTimerEntry;

typedef void ( *InterruptPriorityLevelChangeListener )( uint32_t previousLevel, uint32_t currentLevel ); 

//--//

extern "C" void LLILUM_main(void);

extern CRITICAL_SECTION g_InterruptsStateLock;

template <int N>
class NVIC_Emulation
{
private:

    class InterruptSource
    {
    public:
        typedef void( *InterruptSignal )(InterruptSource& source);

        enum InterruptState
        {
            Disabled = 0,
            Enabled,
            Pending,
            Active,
        };

        //--//

        //
        // State
        //

        InterruptState   State;
        int32_t          Priority;
        InterruptSignal  Handler;

        //--//

        InterruptSource( )
        {
            State    = InterruptState::Enabled;
            Priority = c_Priority__NeverDisabled;
            Handler  = &DefaultHandler;
        }

        bool IsDisabled( ) { return this->State == InterruptState::Disabled; }
        bool IsEnabled ( ) { return this->State == InterruptState::Enabled; }
        bool IsPending ( ) { return this->State == InterruptState::Pending; }
        bool IsActive  ( ) { return this->State == InterruptState::Active; }

        void SetDisabled( ) { this->State = InterruptState::Disabled; }
        void SetEnabled ( ) { this->State = InterruptState::Enabled; }
        void SetPending ( ) { this->State = InterruptState::Pending; }
        void SetActive  ( ) { this->State = InterruptState::Active; }

        void SetHandler( InterruptSignal signal ) { this->Handler = signal; }

        static void DefaultHandler( InterruptSource& source )
        {
            LLOS_Die( );
        }
    };

    //--//

    std::array<InterruptSource, N>                      m_sources;
    HANDLE                                              m_dispatcher;
    HANDLE                                              m_dispatcherEvent;
    std::deque<InterruptPriorityLevelChangeListener>    m_priorityLevelChangeListeners;
    CRITICAL_SECTION                                    m_lock;

public:

    //
    // Constructors
    //

    NVIC_Emulation( )
    {
        InitializeCriticalSection( &g_InterruptsStateLock );
        InitializeCriticalSection( &m_lock );

        m_dispatcherEvent = CreateEvent ( NULL, FALSE, FALSE, NULL );
        m_dispatcher      = CreateThread( NULL, 1024 * 1024, InterruptThreadProc, this, 0, NULL );
    }

    //
    // CMSIS functionality
    //

    void Enable( int32_t irq )
    {
        irq += 15;

        _ASSERT( irq < N );

        EnterCriticalSection( &m_lock );

        if(m_sources[ irq ].IsDisabled( ))
        {
            m_sources[ irq ].State = InterruptSource::InterruptState::Enabled;
        }

        LeaveCriticalSection( &m_lock );
    }

    void Disable( int32_t irq )
    {
        irq += 15;

        _ASSERT( irq < N );

        EnterCriticalSection( &m_lock );

        m_sources[ irq ].State = InterruptSource::InterruptState::Disabled;

        ProcessChange( m_sources[ irq ] );

        LeaveCriticalSection( &m_lock );
    }

    void SetPending( int32_t irq )
    {
        irq += 15;

        _ASSERT( irq < N );

        EnterCriticalSection( &m_lock );

        if(m_sources[ irq ].State >= InterruptSource::InterruptState::Enabled)
        {
            m_sources[ irq ].State = InterruptSource::InterruptState::Pending;
        }

        ProcessChange( m_sources[ irq ] );

        LeaveCriticalSection( &m_lock );
    }

    void ClearPending( int32_t irq )
    {
        irq += 15;

        _ASSERT( irq < N );

        EnterCriticalSection( &m_lock );

        m_sources[ irq ].State = InterruptSource::InterruptState::Enabled;

        ProcessChange( m_sources[ irq ] );

        LeaveCriticalSection( &m_lock );
    }

    bool GetActive( int32_t irq )
    {
        irq += 15;

        _ASSERT( irq < N );

        return m_sources[ irq ].State == InterruptSource::InterruptState::Active ? true : false;
    }

    void SetPriority( int32_t irq, int32_t  pri )
    {
        irq += 15;

        _ASSERT( irq < N );

        EnterCriticalSection( &m_lock );

        m_sources[ irq ].Priority = pri;

        ProcessChange( m_sources[ irq ] );

        LeaveCriticalSection( &m_lock );
    }

    void GetPriority( int32_t irq, int32_t *pri )
    {
        irq += 15;

        _ASSERT( irq < N );

        *pri = m_sources[ irq ].Priority;
    }

    void SetPriorityGrouping( uint32_t priority_group )
    {
        LLOS_Die( );
    }

    //
    // Priority change ND signaling
    //

    void DispatchSysTick( )
    {
        g_EmulatedSystemTimer.ReleaseDispatcher( ); 
    }

    void SignalPriorityLevelChange( uint32_t previousLevel, uint32_t currentLevel )
    {
        for(auto &l : m_priorityLevelChangeListeners)
        {
            l( previousLevel, currentLevel );
        }
    }

    void RegisterForPriorityLevelChangelistener( InterruptPriorityLevelChangeListener listener )
    {
        m_priorityLevelChangeListeners.push_back( listener );
    }

private:

    void ProcessChange( InterruptSource& source )
    {
        if(source.IsPending( ))
        {
            SetEvent( m_dispatcherEvent );
        }
    }

    static DWORD WINAPI InterruptThreadProc( LPVOID lpThreadParameter )
    {
        NVIC_Emulation& nvic = *(NVIC_Emulation*)lpThreadParameter;

        while(true)
        {
            //
            // wait for one interrupt to become pending
            //
            WaitForSingleObject( nvic.m_dispatcherEvent, INFINITE );

            //
            // Find highest priority interrupts that is active, repeated
            // scanning allows easy clearing of active interrupts before 
            // dispatching happens
            //

            EnterCriticalSection( &nvic.m_lock );

            bool fDispatch = true;
            do
            {

                int32_t          highestPriority = 0x7FFFFFFF;
                InterruptSource* activeIsr = NULL;
                for(int i = 0; i < N; ++i)
                {
                    InterruptSource& source = nvic.m_sources[ i ];

                    if(source.IsPending( ))
                    {
                        if(source.Priority < highestPriority)
                        {
                            highestPriority = source.Priority;
                            activeIsr        = &source;
                        }
                    }
                }

                //
                // Dispatch
                //
                if(activeIsr != NULL)
                {
                    activeIsr->State = InterruptSource::InterruptState::Active;
                    activeIsr->Handler( *activeIsr );
                    activeIsr->State = InterruptSource::InterruptState::Enabled;
                }
                else
                {
                    fDispatch = false;
                }

            } while(fDispatch);

            LeaveCriticalSection( &nvic.m_lock );
        }
    }
};

//--//

extern NVIC_Emulation<255> g_EmulatedNVIC;

//--//

extern SystemTimer_Emulation g_EmulatedSystemTimer;

class SystemTimer_Emulation
{
    std::deque<LlosTimerEntry*> m_timers;
    std::set<LlosTimerEntry*>   m_pendingTimers;
    HANDLE                      m_dispatchEvent;
    HANDLE                      m_dispatchThread;
    CRITICAL_SECTION            m_lock;

public:
    SystemTimer_Emulation( )
    {
        InitializeCriticalSection( &m_lock );

    }

    void Initialize( )
    {
        m_dispatchEvent  = CreateEvent ( NULL, FALSE, FALSE, NULL );
        m_dispatchThread = CreateThread( NULL, 1024 * 1024, &DispatchTimerEventProcedure, this, 0, NULL );
        SetThreadPriority( m_dispatchThread, THREAD_PRIORITY_ABOVE_NORMAL ); 

        g_EmulatedNVIC.RegisterForPriorityLevelChangelistener( &ProcessInterruptPriorityChange );
    }

    void RegisterTimer( LlosTimerEntry* entry )
    {
        EnterCriticalSection( &m_lock );

        m_timers.push_back( entry );

        LeaveCriticalSection( &m_lock );
    }

    void DeregisterTimer( LlosTimerEntry* entry )
    {
        EnterCriticalSection( &m_lock );

        for(auto it = m_timers.begin( ); it != m_timers.end( ); ++it)
        {
            if(*it == entry)
            {
                m_timers.erase( it );

                break;
            }
        }

        LeaveCriticalSection( &m_lock );
    }

    void ReleaseDispatcher( )
    {
        SetEvent( m_dispatchEvent );
    }

    static void ProcessInterruptPriorityChange( uint32_t previousLevel, uint32_t currentLevel )
    {
        SystemTimer_Emulation* pThis = (SystemTimer_Emulation*)&g_EmulatedSystemTimer;

        if(previousLevel <= c_Priority__SystemTimer && currentLevel > c_Priority__SystemTimer)
        {
            SetEvent( pThis->m_dispatchEvent );
        }
    }

    static DWORD WINAPI DispatchTimerEventProcedure( LPVOID lpThreadParameter )
    {
        SystemTimer_Emulation* pThis = (SystemTimer_Emulation*)lpThreadParameter;

        while(true)
        {
            bool fDispacth = false;

            switch(WaitForSingleObject( pThis->m_dispatchEvent, INFINITE ))
            {
            case WAIT_OBJECT_0:
                fDispacth = true;
                break;

            default:
                LLOS_Die( );
                break;
            }

            if(fDispacth)
            {
                EnterCriticalSection( &pThis->m_lock );

                std::deque<LlosTimerEntry*> activeTimers( pThis->m_pendingTimers.size( ) );

                std::move( pThis->m_pendingTimers.cbegin( ), pThis->m_pendingTimers.cend( ), activeTimers.rbegin( ) );

                pThis->m_pendingTimers.clear( );

                EnterCriticalSection( &g_InterruptsStateLock ); 
                ConvertThreadToFiber( NULL );

                for(auto& entry : activeTimers)
                {
                    wchar_t buffer[ 128 ];
                    _snwprintf_s( buffer, 128, L"Dispatch timer: %p, expired: %lld\r\n", entry, entry->expiredTime );
                    LLOS_DEBUG_LogText( buffer, 128 );


                    entry->callback( entry->callbackContext, entry->expiredTime );

                }

                ConvertFiberToThread( );
                LeaveCriticalSection( &g_InterruptsStateLock );

                LeaveCriticalSection( &pThis->m_lock );
            }
        }

        return 0;
    }

    static DWORD WINAPI TimerWaitProcedure( LPVOID lpThreadParameter )
    {
        LlosTimerEntry*        pTimer = (LlosTimerEntry*)lpThreadParameter;
        SystemTimer_Emulation* pThis  = (SystemTimer_Emulation*)pTimer->timer;

        if(pTimer == nullptr)
        {
            return E_INVALIDARG;
        }

        while(!pTimer->fCancelled)
        {
            bool fDispatchable = false;

            switch(WaitForSingleObject( pTimer->hndEvent, (DWORD)pTimer->waitTime / 1000 ))
            {
            case WAIT_OBJECT_0:
                break;

            case WAIT_TIMEOUT:
            {
                pTimer->expiredTime = LLOS_SYSTEM_TIMER_GetTicks( pTimer );
                pTimer->waitTime    = INFINITE;


                wchar_t buffer[ 128 ];
                _snwprintf_s( buffer, 128, L"Expired timer: %p, expired: %lld\r\n", pTimer, pTimer->expiredTime );
                LLOS_DEBUG_LogText( buffer, 128 );

                fDispatchable = true;
            }
            break;

            default:
                LLOS_Die( );
                break;
            }

            if(fDispatchable)
            {
                EnterCriticalSection( &pThis->m_lock );

                std::set<LlosTimerEntry*>::iterator it = pThis->m_pendingTimers.find( pTimer );

                if(it == pThis->m_pendingTimers.end( ))
                {
                    wchar_t buffer[ 128 ];
                    _snwprintf_s( buffer, 128, L"Pending timer: %p, expired: %lld\r\n", pTimer, pTimer->expiredTime );
                    LLOS_DEBUG_LogText( buffer, 128 );

                    pThis->m_pendingTimers.insert( pTimer );
                }

                if(g_InterruptsPriority > c_Priority__SysTick)
                {
                    SetEvent( pThis->m_dispatchEvent );
                }

                LeaveCriticalSection( &pThis->m_lock );
            }
        }

        return 0;
    }
};

//--//

extern SystemTimer_Emulation g_EmulatedSystemTimer;

//--//

#endif // __LLOS_WIN32_H__ 
