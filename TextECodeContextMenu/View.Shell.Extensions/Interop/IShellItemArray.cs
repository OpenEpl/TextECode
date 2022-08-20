// Copyright (c) William Kent. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace View.Shell.Extensions.Interop
{
    [ComImport]
    [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemArray
    {
        [Obsolete("Do not use.", true)]
        void BindToHandler();

        [Obsolete("Do not use.", true)]
        void GetPropertyStore();

        [Obsolete("Do not use.", true)]
        void GetPropertyDescriptionList();

        [Obsolete("Do not use.", true)]
        void GetAttributes();

        void GetCount(out int count);

        void GetItemAt(int index, out IShellItem item);

        [Obsolete("Do not use.", true)]
        void EnumItems();
    }

}
