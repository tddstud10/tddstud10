using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.Common;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    public class VsStatusBarIconHost : ObservableObject
    {
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

                _runState = value;
                RaisePropertyChanged(() => RunState);
            }
        }

        private VsStatusBarIconHost()
        {
        }

        public static VsStatusBarIconHost CreateAndInjectIntoVsStatusBar()
        {
            Logger.I.LogInfo("Attempting to inject icon into VS' status bar.");
            StatusBar statusBarElement = FindVsStatusBar();

            VsStatusBarIconHost iconHost = new VsStatusBarIconHost();
            iconHost._statusBarThreadDispatcher = statusBarElement.Dispatcher;

            iconHost.InvokeAsyncOnStatusBarThread(new Action(() =>
            {
                iconHost.InjectStatusBarIcon(statusBarElement);
            }));

            return iconHost;
        }

        public void InvokeAsyncOnStatusBarThread(Action action)
        {
            _statusBarThreadDispatcher.InvokeAsync(action);
        }

        private void InjectStatusBarIcon(StatusBar statusBarElement)
        {
            Logger.I.LogInfo("... In status bar thread.");
            var rootDockPanel = (DockPanel)VisualTreeHelper.GetChild(statusBarElement, 0);
            try
            {
                rootDockPanel.LastChildFill = false;
                var sbItem = new StatusBarItem();
                sbItem.Visibility = Visibility.Visible;
                var sbIcon = new StatusBarNotificationIcon();
                sbIcon.Visibility = Visibility.Visible;
                DockPanel.SetDock(sbItem, Dock.Right);

                BindStatusIconProperties(sbIcon);

                sbItem.Content = sbIcon;
                sbItem.Margin = new Thickness(0);
                sbItem.Padding = new Thickness(0);
                rootDockPanel.Children.Insert(0, sbItem);
                Logger.I.LogInfo("... StatusBarNotificationIcon successfully injected.");
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

        private void BindStatusIconProperties(StatusBarNotificationIcon sbIcon)
        {
            var aBinding = CreateBindingForRunStateProperty<RunStateToAnimationStateConverter>();
            sbIcon.SetBinding(StatusBarNotificationIcon.AnimateProperty, aBinding);
            var cBinding = CreateBindingForRunStateProperty<RunStateToIconColorConverter>();
            sbIcon.SetBinding(StatusBarNotificationIcon.IconColorProperty, cBinding);
            var tBinding = CreateBindingForRunStateProperty<RunStateToIconTextConverter>();
            sbIcon.SetBinding(StatusBarNotificationIcon.IconTextProperty, tBinding);
        }

        private static StatusBar FindVsStatusBar()
        {
            Window mainWindow = Application.Current.MainWindow;
            var contentPresenter = VisualTreeHelper.GetChild(mainWindow, 0);
            Grid rootGrid = (Grid)VisualTreeHelper.GetChild(contentPresenter, 0);
            DockPanel statusBarPanel = (DockPanel)VisualTreeHelper.GetChild(rootGrid, 3);
            var statusBarContainer = statusBarPanel.Children[1];
            Logger.I.LogInfo("... Obtained statusBarContainer.");

            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == "Microsoft.VisualStudio.Shell.UI.Internal, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").First();
            Type type = asm.GetType("Microsoft.VisualStudio.PlatformUI.WorkerThreadStatusBarContainer");
            FieldInfo statusBarElementField = type.GetField("statusBarElement", BindingFlags.Instance | BindingFlags.NonPublic);
            StatusBar statusBarElement = (StatusBar)statusBarElementField.GetValue(statusBarContainer);
            Logger.I.LogInfo("... Obtained statusBarElement.");
            return statusBarElement;
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
