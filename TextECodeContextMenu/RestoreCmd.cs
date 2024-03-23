using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using View.Shell.Extensions;
using View.Shell.Extensions.Interop;

namespace TextECodeContextMenu
{
    [ComVisible(true)]
    [Guid("ea7015ff-6cf8-4b6a-b0dd-0f9ca9553b96")]
    public class RestoreCmd : ExplorerCommandBase
    {
        public override ExplorerCommandFlag Flags => ExplorerCommandFlag.Default;

        public override ExplorerCommandState GetState(IEnumerable<string> selectedFiles)
        {
            return ExplorerCommandState.Enabled;
        }

        public override string GetTitle(IEnumerable<string> selectedFiles)
        {
            return "还原为易源码";
        }

        public override string GetToolTip(IEnumerable<string> selectedFiles)
        {
            throw new NotImplementedException();
        }

        public override void Invoke(IEnumerable<string> selectedFiles)
        {
            foreach (var item in selectedFiles)
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "TextECode.exe"
                };
                startInfo.ArgumentList.Add("restore");
                startInfo.ArgumentList.Add(item);
                using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start TextECode.exe");
                process.WaitForExit();
            }
        }
    }
}
