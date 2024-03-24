#pragma once
#include <ShObjIdl_core.h>
#include <unknwn.h>
#include <winrt/Windows.Foundation.h>
namespace OpenEpl::TextECode::ShellExtension
{
struct DECLSPEC_UUID("e77317d4-6e4f-4703-940b-a775267fce10") ExplorerCommandRootClassFactory
    : public winrt::implements<ExplorerCommandRootClassFactory, IClassFactory>
{
  public:
    HRESULT STDMETHODCALLTYPE CreateInstance(_In_opt_ IUnknown *pUnkOuter, _In_ REFIID riid,
                                             _COM_Outptr_ void **ppvObject) noexcept override;
    HRESULT STDMETHODCALLTYPE LockServer(_In_ BOOL fLock) noexcept override;
};
} // namespace OpenEpl::TextECode::ShellExtension
