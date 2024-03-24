#pragma once
#include <ShObjIdl_core.h>
#include <unknwn.h>
#include <winrt/Windows.Foundation.h>
namespace OpenEpl::TextECode::ShellExtension
{
struct RestoreCommand : public winrt::implements<RestoreCommand, IExplorerCommand>
{
    HRESULT STDMETHODCALLTYPE GetTitle(_In_opt_ IShellItemArray *psiItemArray, _Outptr_ LPWSTR *ppszName);

    HRESULT STDMETHODCALLTYPE GetIcon(_In_opt_ IShellItemArray *psiItemArray, _Outptr_ LPWSTR *ppszIcon);

    HRESULT STDMETHODCALLTYPE GetToolTip(_In_opt_ IShellItemArray *psiItemArray, _Outptr_ LPWSTR *ppszInfotip);

    HRESULT STDMETHODCALLTYPE GetCanonicalName(_Out_ GUID *pguidCommandName);

    HRESULT STDMETHODCALLTYPE GetState(_In_opt_ IShellItemArray *psiItemArray, _In_ BOOL fOkToBeSlow,
                                       _Out_ EXPCMDSTATE *pCmdState);

    HRESULT STDMETHODCALLTYPE Invoke(_In_opt_ IShellItemArray *psiItemArray, _In_opt_ IBindCtx *pbc);

    HRESULT STDMETHODCALLTYPE GetFlags(_Out_ EXPCMDFLAGS *pFlags);

    HRESULT STDMETHODCALLTYPE EnumSubCommands(_Outptr_ IEnumExplorerCommand **ppEnum);
};
} // namespace OpenEpl::TextECode::ShellExtension