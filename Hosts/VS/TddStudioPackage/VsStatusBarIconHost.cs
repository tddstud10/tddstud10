using GalaSoft.MvvmLight;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Hosts.Common.StatusBar;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    public class VsStatusBarIconHost : ObservableObject
    {
        private int _injectControlAttempts;

        private Dispatcher _statusBarThreadDispatcher;

        private RunState _runState = RunState.Initial;
        public RunState RunState
        {
            get { return _runState; }
            set
            {
                if (_runState == value)
                {
                    return;
                }

                _statusBarThreadDispatcher.InvokeAsync(
                    () =>
                    {
                        _runState = value;
                        RaisePropertyChanged(() => RunState);
                    });
            }
        }

        public static VsStatusBarIconHost CreateAndInjectIntoVsStatusBar()
        {
            Logger.I.LogInfo("Attempting to inject icon into VS' status bar.");

            VsStatusBarIconHost iconHost = new VsStatusBarIconHost();
            iconHost.InjectControl();

            return iconHost;
        }

        private VsStatusBarIconHost()
        {
        }

        private static bool IsUnmodifiedStatusBar(StatusBar statusBar)
        {
            DependencyObject child = VisualTreeHelper.GetChild(statusBar, 0);
            return child is DockPanel;
        }

        private void InjectControl()
        {
            Window mainWindow = Application.Current.MainWindow;
            FrameworkElement frameworkElement = FindChild(mainWindow, "ResizeGripControl") as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }
            DockPanel dockPanel = frameworkElement.Parent as DockPanel;
            if (dockPanel == null)
            {
                return;
            }
            FrameworkElement frameworkElement2 = FindStatusBarContainer(dockPanel);
            if (frameworkElement2 == null)
            {
                return;
            }
            if (!TryInjectMonitorControl(frameworkElement2))
            {
                ScheduleRetryInjectMonitorControl(frameworkElement2);
            }
        }

        private void ScheduleRetryInjectMonitorControl(object statusBarContainer)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer.Tag = statusBarContainer;
            dispatcherTimer.Tick += new EventHandler(OnRetryTimerTick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1.0);
            dispatcherTimer.Start();
        }

        private void OnRetryTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
            object tag = dispatcherTimer.Tag;
            if (TryInjectMonitorControl(tag) || ++_injectControlAttempts == 5)
            {
                dispatcherTimer.Tick -= new EventHandler(OnRetryTimerTick);
                dispatcherTimer.Tag = null;
                dispatcherTimer.Stop();
            }
        }

        private bool TryInjectMonitorControl(object statusBarContainer)
        {
            FieldInfo field = statusBarContainer.GetType().GetField("statusBarElement", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return false;
            }
            StatusBar statusBar = field.GetValue(statusBarContainer) as StatusBar;
            if (statusBar == null)
            {
                return false;
            }
            _statusBarThreadDispatcher = statusBar.Dispatcher;
            _statusBarThreadDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<StatusBar>(InjectStatusBarIcon), statusBar);
            return true;
        }

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject dependencyObject = VisualTreeHelper.GetChild(parent, i);
                FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }
                dependencyObject = FindChild(dependencyObject, childName);
                if (dependencyObject != null)
                {
                    return dependencyObject;
                }
            }
            return null;
        }

        private static FrameworkElement FindStatusBarContainer(Panel panel)
        {
            foreach (object current in panel.Children)
            {
                FrameworkElement frameworkElement = current as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == "StatusBarContainer")
                {
                    return frameworkElement;
                }
            }
            return null;
        }

        private void InjectStatusBarIcon(StatusBar statusBar)
        {
            Logger.I.LogInfo("... In status bar thread.");
            if (!IsUnmodifiedStatusBar(statusBar))
            {
                return;
            }

            var rootDockPanel = (DockPanel)VisualTreeHelper.GetChild(statusBar, 0);
            try
            {
                rootDockPanel.LastChildFill = false;
                var sbItem = new StatusBarItem();
                sbItem.Visibility = Visibility.Visible;
                var sbIcon = new NotificationIcon();
                sbIcon.Visibility = Visibility.Visible;
                DockPanel.SetDock(sbItem, Dock.Right);

                BindStatusIconProperties(sbIcon);

                sbItem.Content = sbIcon;
                sbItem.Margin = new Thickness(0);
                sbItem.Padding = new Thickness(0);
                rootDockPanel.Children.Insert(0, sbItem);
                Logger.I.LogInfo("... StatusBar NotificationIcon successfully injected.");
            }
            catch (Exception e)
            {
                Logger.I.LogError("... Got an exception {0}.", e);
            }
            finally
            {
                rootDockPanel.LastChildFill = true;
            }
            Logger.I.LogInfo("... Exiting from status bar thread.");
        }

        private void BindStatusIconProperties(NotificationIcon sbIcon)
        {
            var aBinding = CreateBindingForRunStateProperty<RunStateToAnimationStateConverter>();
            sbIcon.SetBinding(NotificationIcon.AnimateProperty, aBinding);
            var cBinding = CreateBindingForRunStateProperty<RunStateToIconColorConverter>();
            sbIcon.SetBinding(NotificationIcon.IconColorProperty, cBinding);
            var tBinding = CreateBindingForRunStateProperty<RunStateToIconTextConverter>();
            sbIcon.SetBinding(NotificationIcon.IconTextProperty, tBinding);
        }

        private Binding CreateBindingForRunStateProperty<T>() where T : IValueConverter, new()
        {
            Binding myBinding = new Binding("RunState")
            {
                Converter = new T(),
                Source = this,
            };
            return myBinding;
        }
    }
}
