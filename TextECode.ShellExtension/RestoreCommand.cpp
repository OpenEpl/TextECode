#define WIN32_LEAN_AND_MEAN
#include "RestoreCommand.h"
#include <Windows.h>
#include <shellapi.h>
#include <shlwapi.h>

namespace OpenEpl::TextECode::ShellExtension
{
HRESULT STDMETHODCALLTYPE RestoreCommand::GetTitle(_In_opt_ IShellItemArray *psiItemArray, _Outptr_ LPWSTR *ppszName)
{
    return SHStrDupW(L"还原为易源码", ppszName);
}

HRESULT STDMETHODCALLTYPE RestoreCommand::GetIcon(_In_opt_ IShellItemArray *psiItemArray, _Outptr_ LPWSTR *ppszIcon)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    *ppszIcon = nullptr;
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::GetToolTip(_In_opt_ IShellItemArray *psiItemArray,
                                                     _Outptr_ LPWSTR *ppszInfotip)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    *ppszInfotip = nullptr;
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::GetCanonicalName(_Out_ GUID *pguidCommandName)
{
    *pguidCommandName = GUID_NULL;
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::GetState(_In_opt_ IShellItemArray *psiItemArray, _In_ BOOL fOkToBeSlow,
                                                   _Out_ EXPCMDSTATE *pCmdState)
{
    UNREFERENCED_PARAMETER(fOkToBeSlow);
    *pCmdState = ECS_HIDDEN;
    if (psiItemArray == nullptr)
    {
        return S_OK;
    }
    DWORD count = 0;
    HRESULT ret = psiItemArray->GetCount(&count);
    if (FAILED(ret))
    {
        return ret;
    }
    for (DWORD i = 0; i < count; i++)
    {
        winrt::com_ptr<IShellItem> psiItem;
        ret = psiItemArray->GetItemAt(i, psiItem.put());
        if (FAILED(ret))
        {
            return ret;
        }
        PWSTR pszName = nullptr;
        ret = psiItem->GetDisplayName(SIGDN_FILESYSPATH, &pszName);
        if (FAILED(ret))
        {
            return ret;
        }
        if (PathMatchSpecW(pszName, L"*.eproject"))
        {
            *pCmdState = ECS_ENABLED;
            CoTaskMemFree(pszName);
            return S_OK;
        }
        CoTaskMemFree(pszName);
    }
    return S_OK;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::Invoke(_In_opt_ IShellItemArray *psiItemArray, _In_opt_ IBindCtx *pbc)
{
    UNREFERENCED_PARAMETER(pbc);
    if (psiItemArray == nullptr)
    {
        return S_OK;
    }
    DWORD count = 0;
    HRESULT ret = psiItemArray->GetCount(&count);
    if (FAILED(ret))
    {
        return ret;
    }
    for (DWORD i = 0; i < count; i++)
    {
        winrt::com_ptr<IShellItem> psiItem;
        ret = psiItemArray->GetItemAt(i, psiItem.put());
        if (FAILED(ret))
        {
            return ret;
        }
        PWSTR pszName = nullptr;
        ret = psiItem->GetDisplayName(SIGDN_FILESYSPATH, &pszName);
        if (FAILED(ret))
        {
            return ret;
        }
        if (PathMatchSpecW(pszName, L"*.eproject"))
        {
            std::wstring command = L"restore \"";
            command += pszName;
            command += L"\"";
            ShellExecuteW(nullptr, L"open", L"TextECode.exe", command.c_str(), nullptr, SW_SHOWNORMAL);
        }
        CoTaskMemFree(pszName);
    }
    return S_OK;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::GetFlags(_Out_ EXPCMDFLAGS *pFlags)
{
    *pFlags = ECF_DEFAULT;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE RestoreCommand::EnumSubCommands(_Outptr_ IEnumExplorerCommand **ppEnum)
{
    return E_NOTIMPL;
}
} // namespace OpenEpl::TextECode::ShellExtension