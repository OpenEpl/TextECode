#pragma once
#include <ShObjIdl_core.h>
#include <unknwn.h>
#include <winrt/Windows.Foundation.h>
namespace OpenEpl::TextECode::ShellExtension
{
struct EnumExplorerCommand : public winrt::implements<EnumExplorerCommand, IEnumExplorerCommand>
{
  private:
    std::vector<winrt::com_ptr<IExplorerCommand>> subcommands;
    size_t index = 0;

  public:
    EnumExplorerCommand(std::vector<winrt::com_ptr<IExplorerCommand>> subcommands);

    HRESULT STDMETHODCALLTYPE Next(_In_ ULONG celt, _Out_writes_(celt) IExplorerCommand **pUICommand,
                                   _Out_opt_ ULONG *pceltFetched);

    HRESULT STDMETHODCALLTYPE Skip(_In_ ULONG celt);

    HRESULT STDMETHODCALLTYPE Reset();

    HRESULT STDMETHODCALLTYPE Clone(_Outptr_ IEnumExplorerCommand **ppEnum);
};
} // namespace OpenEpl::TextECode::ShellExtension