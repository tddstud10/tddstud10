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

        public bool Animate
        {
            get { return (bool)GetValue(AnimateProperty); }
            set { SetValue(AnimateProperty, value); }
        }

        public static readonly DependencyProperty AnimateProperty =
            DependencyProperty.Register("Animate", typeof(bool), typeof(StatusBarNotificationIcon), new PropertyMetadata(RunStateToAnimationStateConverter.AnimationOff));

        public SolidColorBrush IconColor
        {
            get { return (SolidColorBrush)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register("IconColor", typeof(SolidColorBrush), typeof(StatusBarNotificationIcon), new PropertyMetadata(new SolidColorBrush(RunStateToIconColorConverter.ColorForUnknown)));

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(StatusBarNotificationIcon), new PropertyMetadata(RunStateToIconTextConverter.TextForUnknown));
    }
}
