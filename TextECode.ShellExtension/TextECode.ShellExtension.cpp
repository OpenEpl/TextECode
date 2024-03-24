#pragma comment(lib, "Shlwapi.lib")

#define WIN32_LEAN_AND_MEAN
#include "TextECode.ShellExtension.h"
#include "ExplorerCommandRootClassFactory.h"
#include <Windows.h>
#include <unknwn.h>
#include <winrt/Windows.Foundation.h>

EXTERN_C HRESULT STDAPICALLTYPE DllCanUnloadNow()
{
    if (winrt::get_module_lock())
    {
        return S_FALSE;
    }

    winrt::clear_factory_cache();
    return S_OK;
}

EXTERN_C HRESULT STDAPICALLTYPE DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID *ppv)
{
    if (!ppv)
    {
        return E_POINTER;
    }

    if (riid != IID_IClassFactory && riid != IID_IUnknown)
    {
        return E_NOINTERFACE;
    }

    try
    {
        if (rclsid == __uuidof(OpenEpl::TextECode::ShellExtension::ExplorerCommandRootClassFactory))
        {
            return winrt::make<OpenEpl::TextECode::ShellExtension::ExplorerCommandRootClassFactory>()->QueryInterface(
                riid, ppv);
        }
        else
        {

            return E_INVALIDARG;
        }
    }
    catch (...)
    {
        return winrt::to_hresult();
    }
}

HMODULE g_hModule = nullptr;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    UNREFERENCED_PARAMETER(lpReserved);

    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH: {
        g_hModule = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
    }
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
