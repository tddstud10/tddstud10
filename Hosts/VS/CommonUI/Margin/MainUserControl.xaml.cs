using System.Windows.Controls;

namespace R4nd0mApps.TddStud10.Hosts.Common.Margin
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public Canvas Canvas
        {
            get { return this.canvas; }
        }

        public MainUserControl()
        {
            InitializeComponent();
        }
    }
}
