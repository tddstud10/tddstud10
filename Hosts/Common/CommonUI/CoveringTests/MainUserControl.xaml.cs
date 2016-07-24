using System.Windows.Controls;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }

        private void Path_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = true;
        }
    }
}
