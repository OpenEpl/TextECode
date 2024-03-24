#include "EnumExplorerCommand.h"

namespace OpenEpl::TextECode::ShellExtension
{
EnumExplorerCommand::EnumExplorerCommand(std::vector<winrt::com_ptr<IExplorerCommand>> subcommands)
    : subcommands(subcommands)
{
}
HRESULT STDMETHODCALLTYPE EnumExplorerCommand::Next(_In_ ULONG celt, _Out_writes_(celt) IExplorerCommand **pUICommand,
                                                    _Out_opt_ ULONG *pceltFetched)
{
    if (pUICommand == nullptr)
    {
        return E_POINTER;
    }
    ULONG fetched = 0;
    for (ULONG i = 0; i < celt; i++)
    {
        if (this->index >= this->subcommands.size())
        {
            break;
        }
        this->subcommands[this->index].copy_to(&pUICommand[i]);
        this->index++;
        fetched++;
    }
    if (pceltFetched)
    {
        *pceltFetched = fetched;
    }
    return (fetched == celt) ? S_OK : S_FALSE;
}

HRESULT STDMETHODCALLTYPE EnumExplorerCommand::Skip(_In_ ULONG celt)
{
    this->index += celt;
    if (this->index > this->subcommands.size())
    {
        this->index = this->subcommands.size();
    }
    return S_OK;
}

HRESULT STDMETHODCALLTYPE EnumExplorerCommand::Reset()
{
    this->index = 0;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE EnumExplorerCommand::Clone(_Outptr_ IEnumExplorerCommand **ppEnum)
{
    return winrt::make<EnumExplorerCommand>(this->subcommands)->QueryInterface(ppEnum);
}
} // namespace OpenEpl::TextECode::ShellExtension