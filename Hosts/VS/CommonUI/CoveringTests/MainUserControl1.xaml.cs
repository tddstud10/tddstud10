using System.Windows.Controls;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class MainUserControl1 : UserControl
    {
        public Canvas Canvas
        {
            get { return this.canvas; }
        }

        public MainUserControl1()
        {
            InitializeComponent();
        }
    }
}
