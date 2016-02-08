#include "LlosWin32.h"
#include <llos_mutex.h>

//--//

int LlosWin32_Main()
{
    // Thread local storage for refcount on global mutex
    LlosThreadData** ppvData = (LlosThreadData**)LocalAlloc(LPTR, sizeof(LlosThreadData) + sizeof(LlosThreadData*));
    *ppvData = (LlosThreadData*)&ppvData[1];
    //TlsSetValue(g_dwTlsIndex, ppvData);

    if (NULL == ConvertThreadToFiber(ppvData))
    {
        return E_OUTOFMEMORY;
    }

    LLILUM_main();

    LocalFree(ppvData);

    return 0;
}
