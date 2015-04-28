using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    // TODO: Move to fs.
    public static class VsUIShellExtensions
    {
        public static MessageBoxResult DisplayMessageBox(this IVsUIShell uiShell, string title, string text)
        {
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
              uiShell.ShowMessageBox(
                0,
                ref clsid,
                title,
                text,
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                out result));

            return MessageBoxResult.OK;
        }
    }
}
