using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace R4nd0mApps.TddStud10.Hosts.Common
{
    public partial class StatusBarNotificationIcon : UserControl
    {
        public StatusBarNotificationIcon()
        {
            InitializeComponent();
        }

        public bool RunState
        {
            get { return (bool)GetValue(RunStateProperty); }
            set { SetValue(RunStateProperty, value); }
        }

        public static readonly DependencyProperty RunStateProperty =
            DependencyProperty.Register("RunState", typeof(bool), typeof(StatusBarNotificationIcon), new PropertyMetadata(false));

        public SolidColorBrush RunStatus
        {
            get { return (SolidColorBrush)GetValue(RunStatusProperty); }
            set { SetValue(RunStatusProperty, value); }
        }

        public static readonly DependencyProperty RunStatusProperty =
            DependencyProperty.Register("RunStatus", typeof(SolidColorBrush), typeof(StatusBarNotificationIcon), new PropertyMetadata(Brushes.LightGray));

        public string RunStepKind
        {
            get { return (string)GetValue(RunStepKindProperty); }
            set { SetValue(RunStepKindProperty, value); }
        }

        public static readonly DependencyProperty RunStepKindProperty =
            DependencyProperty.Register("RunStepKind", typeof(string), typeof(StatusBarNotificationIcon), new PropertyMetadata("?"));
    }
}
