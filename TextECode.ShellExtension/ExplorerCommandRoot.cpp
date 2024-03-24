#define WIN32_LEAN_AND_MEAN
#include "ExplorerCommandRoot.h"
#include "EnumExplorerCommand.h"
#include "GenerateCommand.h"
#include "RestoreCommand.h"
#include "TextECode.ShellExtension.h"
#include <Windows.h>
#include <shlwapi.h>

namespace OpenEpl::TextECode::ShellExtension
{
HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetTitle(_In_opt_ IShellItemArray *psiItemArray,
                                                        _Outptr_ LPWSTR *ppszName)
{
    return SHStrDupW(L"TextECode", ppszName);
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetIcon(_In_opt_ IShellItemArray *psiItemArray,
                                                       _Outptr_ LPWSTR *ppszIcon)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    auto str = (LPWSTR)CoTaskMemAlloc(MAX_PATH * sizeof(WCHAR));
    if (str == nullptr)
    {
        return E_OUTOFMEMORY;
    }
    if (GetModuleFileNameW(g_hModule, str, MAX_PATH) == 0)
    {
        CoTaskMemFree(str);
        return E_FAIL;
    }
    PathRemoveFileSpecW(str);
    PathRemoveFileSpecW(str);
    wcscat_s(str, MAX_PATH, L"\\TextECodeCLI\\TextECode.exe,0");
    *ppszIcon = str;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetToolTip(_In_opt_ IShellItemArray *psiItemArray,
                                                          _Outptr_ LPWSTR *ppszInfotip)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    *ppszInfotip = nullptr;
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetCanonicalName(_Out_ GUID *pguidCommandName)
{
    *pguidCommandName = GUID_NULL;
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetState(_In_opt_ IShellItemArray *psiItemArray, _In_ BOOL fOkToBeSlow,
                                                        _Out_ EXPCMDSTATE *pCmdState)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    UNREFERENCED_PARAMETER(fOkToBeSlow);
    *pCmdState = ECS_ENABLED;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::Invoke(_In_opt_ IShellItemArray *psiItemArray, _In_opt_ IBindCtx *pbc)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    UNREFERENCED_PARAMETER(pbc);
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::GetFlags(_Out_ EXPCMDFLAGS *pFlags)
{
    *pFlags = ECF_HASSUBCOMMANDS;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE ExplorerCommandRoot::EnumSubCommands(_Outptr_ IEnumExplorerCommand **ppEnum)
{
    std::vector<winrt::com_ptr<IExplorerCommand>> subcommands;
    subcommands.push_back(winrt::make<GenerateCommand>());
    subcommands.push_back(winrt::make<RestoreCommand>());
    return winrt::make<EnumExplorerCommand>(subcommands)->QueryInterface(ppEnum);
}
}; // namespace OpenEpl::TextECode::ShellExtension