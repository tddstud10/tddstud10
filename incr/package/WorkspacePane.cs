using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using MsVsShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    [Guid("F0E1E9A1-9860-484d-AD5D-367D79AABF55")]
    class WorkspacePane : MsVsShell.ToolWindowPane
    {
        private Control control = null;

        public WorkspacePane()
            : base(null)
        {
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            control = new WorkspacePaneControl();
            this.Content = control;
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            PackageToolWindow package = (PackageToolWindow)this.Package;

            this.Caption = package.GetResourceString("@110");
        }
    }
}
