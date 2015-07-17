using System;
using System.Runtime.InteropServices;

using MsVsShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    [Guid("F0E1E9A1-9860-484d-AD5D-367D79AABF55")]
    class DynamicWindowPane : MsVsShell.ToolWindowPane
    {
        private DynamicWindowWPFControl control = null;

        public DynamicWindowPane()
            : base(null)
        {
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            control = new DynamicWindowWPFControl();
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
