using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// Interaction logic for WorkspacePaneControl.xaml
    /// </summary>
    public partial class WorkspacePaneControl : UserControl
    {
        static WorkspacePaneControl()
        {
            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(
                    new Uri("ToolWindow;component/Resources/WorkspacePaneControlResourceDictionary.xaml",
                    UriKind.Relative)) as ResourceDictionary);
        }

        public WorkspacePaneControl()
        {
            InitializeComponent();
        }
    }
}
