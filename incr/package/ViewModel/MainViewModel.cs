using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _eventLog;

        public string EventLog
        {
            get { return _eventLog; }
            set
            {
                if (_eventLog == value)
                {
                    return;
                }

                _eventLog = value;
                RaisePropertyChanged(() => EventLog);
            }
        }

        private WorkspaceViewModel _workspace;

        public WorkspaceViewModel Workspace
        {
            get { return _workspace; }
            set
            {
                _workspace = value;
            }
        }

        public RelayCommand LoadUnloadWorkspace { get; set; }

        public MainViewModel()
        {
            Workspace = new WorkspaceViewModel();

            EventLog = "This is the event log...";

            LoadUnloadWorkspace = new RelayCommand(
                async () =>
                {
                    if (_workspace.State == WorkspaceState.Unloaded)
                    {
                        var wl = new WorkspaceLoader(Services.GetService<EnvDTE.DTE>().Solution);
                        wl.LoadComplete += async (s, w) => await _workspace.Load(w);

                        wl.Load();
                    }
                    else if (_workspace.State == WorkspaceState.Loaded)
                    {
                        await _workspace.Unload();
                    }
                    else
                    {
                        // Do nothing!
                    }
                },
                () => _workspace.State != WorkspaceState.Loading);
        }
    }
}
