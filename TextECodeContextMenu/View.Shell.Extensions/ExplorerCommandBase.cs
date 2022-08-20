// Copyright (c) William Kent. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using View.Shell.Extensions.Interop;

namespace View.Shell.Extensions
{
    /// <summary>
    /// This class is the base class of all C# Windows Shell commands.
    /// </summary>
    /// <remarks>
    /// This class abstracts away the underlying COM interfaces,
    /// and exposes a more C#-like API.
    /// </remarks>
    public abstract class ExplorerCommandBase : IExplorerCommand
    {
        /// <summary>
        /// Gets a <see cref="Guid"/> that uniquely identifies the command.
        /// </summary>
        public virtual Guid CanonicalName => this.GetType().GUID;

        /// <summary>
        /// Gets a <see cref="ExplorerCommandFlag"/> instance that describes how the command should be displayed.
        /// </summary>
        public abstract ExplorerCommandFlag Flags { get; }

        /// <summary>
        /// Gets the sub-commands of this command, or <see langword="null"/> if there are none.
        /// </summary>
        public virtual IEnumerable<ExplorerCommandBase>? SubCommands => null;

        /// <summary>
        /// Gets the title of the command, to be displayed to the user.
        /// </summary>
        /// <param name="selectedFiles">
        /// The full (rooted) paths to the selected files.
        /// </param>
        /// <returns>
        /// The title to be displayed.
        /// </returns>
        public abstract string? GetTitle(IEnumerable<string> selectedFiles);

        /// <summary>
        /// Gets the tooltip string, to be displayed to the user.
        /// </summary>
        /// <param name="selectedFiles">
        /// The full (rooted) paths to the selected files.
        /// </param>
        /// <returns>
        /// The tooltip string to be displayed, or <see langword="null"/> if there should be none.
        /// </returns>
        public abstract string? GetToolTip(IEnumerable<string> selectedFiles);

        /// <summary>
        /// Gets the state of the command with respect to the <paramref name="selectedFiles"/>.
        /// </summary>
        /// <param name="selectedFiles">
        /// The full (rooted) paths to the selected files.
        /// </param>
        /// <returns>
        /// A <see cref="ExplorerCommandState"/> value.
        /// </returns>
        public abstract ExplorerCommandState GetState(IEnumerable<string> selectedFiles);

        /// <summary>
        /// Called when the command is clicked.
        /// </summary>
        /// <param name="selectedFiles">
        /// The full (rooted) paths to the selected files.
        /// </param>
        public abstract void Invoke(IEnumerable<string> selectedFiles);

        private static IEnumerable<string> ConvertShellItemArray(IShellItemArray itemArray)
        {
            itemArray.GetCount(out var count);
            string[] paths = new string[count];

            for (int i = 0; i < count; i++)
            {
                itemArray.GetItemAt(i, out IShellItem item);
                item.GetDisplayName(SIGDN.FILESYSPATH, out var name);
                paths[i] = name;
            }

            return paths;
        }

#pragma warning disable SA1600 // Elements should be documented (not public API)
#pragma warning disable SA1202 // Elements should be ordered by access (wart)
        void IExplorerCommand.GetTitle(IShellItemArray itemArray, out string? title)
        {
            title = this.GetTitle(ConvertShellItemArray(itemArray));
        }

        void IExplorerCommand.GetIcon(IShellItemArray itemArray, out string? resourceString)
        {
            // Deliberately not implemented, as Win32 icon resource strings are not supported well by .NET.
            throw new NotImplementedException();
        }

        void IExplorerCommand.GetToolTip(IShellItemArray itemArray, out string? tooltip)
        {
            tooltip = this.GetToolTip(ConvertShellItemArray(itemArray));
        }

        void IExplorerCommand.GetCanonicalName(out Guid guid)
        {
            guid = this.CanonicalName;
        }

        void IExplorerCommand.GetState(IShellItemArray itemArray, bool okToBeShow, out ExplorerCommandState commandState)
        {
            commandState = this.GetState(ConvertShellItemArray(itemArray));
        }

        void IExplorerCommand.Invoke(IShellItemArray itemArray, object bindCtx)
        {
            this.Invoke(ConvertShellItemArray(itemArray));
        }

        void IExplorerCommand.GetFlags(out int flags)
        {
            flags = (int)this.Flags;

            const int ECF_HASSUBCOMMANDS = 0x1;
            bool hasSubCommands = this.SubCommands?.Any() ?? false;
            if (hasSubCommands)
            {
                flags |= ECF_HASSUBCOMMANDS;
            }
        }

        int IExplorerCommand.EnumSubCommands(out IEnumExplorerCommand? commandEnum)
        {
            const int S_OK = 0, S_FALSE = 1;
            IEnumerable<ExplorerCommandBase>? subcommands = this.SubCommands;

            if (subcommands != null && subcommands.Any())
            {
                commandEnum = new EnumExplorerCommandImpl(subcommands);
                return S_OK;
            }
            else
            {
                commandEnum = null;
                return S_FALSE;
            }
        }
#pragma warning restore SA1600

        private class EnumExplorerCommandImpl : IEnumExplorerCommand
        {
            private readonly ExplorerCommandBase[] commands;
            private uint index;

            public EnumExplorerCommandImpl(IEnumerable<ExplorerCommandBase> commands)
            {
                this.commands = commands.ToArray();
            }

            public int Next(uint elementCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] out IExplorerCommand[] commands, out uint fetched)
            {
                const int S_OK = 0, S_FALSE = 1;

                fetched = Math.Min(elementCount, (uint)this.commands.Length - this.index);
                if (fetched == 0)
                {
                    commands = Array.Empty<IExplorerCommand>();
                    return S_FALSE;
                }

                commands = new IExplorerCommand[fetched];

                for (uint i = 0; i < fetched; i++)
                {
                    commands[i] = this.commands[this.index + i];
                }

                this.index += fetched;
                return this.index == this.commands.Length ? S_FALSE : S_OK;
            }

            public void Skip(uint count)
            {
                this.index = Math.Min(this.index + count, (uint)this.commands.Length);
            }

            public void Reset()
            {
                this.index = 0;
            }

            public void Clone(out IEnumExplorerCommand copy)
            {
                EnumExplorerCommandImpl copyImpl = new EnumExplorerCommandImpl(this.commands);
                copyImpl.index = this.index;
                copy = copyImpl;
            }
        }
    }
}
