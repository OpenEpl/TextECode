using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using View.Shell.Extensions;
using View.Shell.Extensions.Interop;

namespace TextECodeContextMenu
{
    [ComVisible(true)]
    [Guid("f481db40-c55e-4d0e-8854-6f92f788f855")]
    public class GenerateCmd : ExplorerCommandBase
    {
        public override ExplorerCommandFlag Flags => ExplorerCommandFlag.Default;

        public override ExplorerCommandState GetState(IEnumerable<string> selectedFiles)
        {
            return ExplorerCommandState.Enabled;
        }

        public override string GetTitle(IEnumerable<string> selectedFiles)
        {
            return "生成文本代码";
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
                startInfo.ArgumentList.Add("generate");
                startInfo.ArgumentList.Add(item);
                using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start TextECode.exe");
                process.WaitForExit();
            }
        }
    }
}
