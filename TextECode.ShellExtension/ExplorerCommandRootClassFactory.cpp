#include "ExplorerCommandRootClassFactory.h"
#include "ExplorerCommandRoot.h"

namespace OpenEpl::TextECode::ShellExtension
{
HRESULT STDMETHODCALLTYPE ExplorerCommandRootClassFactory::CreateInstance(_In_opt_ IUnknown *pUnkOuter,
                                                                          _In_ REFIID riid,
                                                                          _COM_Outptr_ void **ppvObject) noexcept
{
    UNREFERENCED_PARAMETER(pUnkOuter);
    try
    {
        return winrt::make<ExplorerCommandRoot>()->QueryInterface(riid, ppvObject);
    }
    catch (...)
    {
        return winrt::to_hresult();
    }
}
HRESULT STDMETHODCALLTYPE ExplorerCommandRootClassFactory::LockServer(_In_ BOOL fLock) noexcept
{
    if (fLock)
    {
        ++winrt::get_module_lock();
    }
    else
    {
        --winrt::get_module_lock();
    }
    return S_OK;
}
} // namespace OpenEpl::TextECode::ShellExtension