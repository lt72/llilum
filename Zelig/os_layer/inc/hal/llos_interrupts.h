//
//    LLILUM OS Abstraction Layer - Interrupts
// 

#ifndef __LLOS_INTERRUPTS_H__
#define __LLOS_INTERRUPTS_H__


//
// !!!! Keep in sync with processorwin32.cs !!!!
//

const uint32_t  c_Priority__MASK               = 0x000000FFu;
const uint32_t  c_Priority__NeverDisabled      = 0x00000000u;
const uint32_t  c_Priority__Highest            = 0x00000001u;
const uint32_t  c_Priority__Lowest             = 0x000000FFu;
const uint32_t  c_Priority__HigherThanAnyWeOwn = 0x00000004u;
const uint32_t  c_Priority__SVCCall            = 0x00000005u;
const uint32_t  c_Priority__Default            = 0x00000007u;
const uint32_t  c_Priority__SystemTimer        = c_Priority__Default;
const uint32_t  c_Priority__SysTick            = c_Priority__Default;
const uint32_t  c_Priority__PendSV             = 0x0000000Eu;


#ifdef __cplusplus
extern "C" {
#endif

#include "llos_types.h"

    HRESULT LLOS_INTERRUPTS_Enable(int32_t irq);
    HRESULT LLOS_INTERRUPTS_Disable(int32_t irq);
    HRESULT LLOS_INTERRUPTS_SetPending(int32_t irq);
    HRESULT LLOS_INTERRUPTS_ClearPending(int32_t irq);
    HRESULT LLOS_INTERRUPTS_GetActive(int32_t irq);
    HRESULT LLOS_INTERRUPTS_SetPriority(int32_t irq, int32_t  pri);
    HRESULT LLOS_INTERRUPTS_GetPriority(int32_t irq, int32_t *pri);
    HRESULT LLOS_INTERRUPTS_SetPriorityGrouping(uint32_t priority_group);

    uint32_t LLOS_INTERRUPTS_GetIsrPriorityLevel( );
    uint32_t LLOS_INTERRUPTS_GetActiveIsrNumber ( );
    uint32_t LLOS_INTERRUPTS_DisableInterruptsWithPriorityLevelLowerOrEqualTo( uint32_t basepri ); 
    void     LLOS_INTERRUPTS_WaitForEvent( ); 

#ifdef __cplusplus
}
#endif

#endif // __LLOS_INTERRUPTS_H__
