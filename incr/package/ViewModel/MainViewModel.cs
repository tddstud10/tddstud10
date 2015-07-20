using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

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

        private Workspace _workspace;

        public Workspace Workspace
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
            if (IsInDesignMode)
            {
                Workspace = new Workspace
                {
                    State = WorkspaceState.Loaded,
                    Projects = new List<Project> 
                    {
                        new Project
                        {
                            Name = "a.csproj",
                            ProjectItems = new List<ProjectItem>
                            {
                                new ProjectItem { Name = "a.cs" },
                                new ProjectItem { Name = "a\\b.cs" },
                            },
                            Projects = new List<Project> 
                            {
                                new Project
                                {
                                    Name = "c.csproj",
                                    ProjectItems = new List<ProjectItem>
                                    {
                                        new ProjectItem { Name = "c.cs" },
                                    },
                                    Projects = new List<Project>()
                                    {
                                        new Project
                                        {
                                            Name = "e.fsproj",
                                            ProjectItems = new List<ProjectItem>
                                            {
                                                new ProjectItem { Name = "e\\e.fs" },
                                            }
                                        },
                                    },
                                },
                            }
                       },
                       new Project
                       {
                            Name = "b.fsproj",
                            ProjectItems = new List<ProjectItem>
                            {
                                new ProjectItem { Name = "b.fs" },
                                new ProjectItem { Name = "b\\c.fs" },
                            },
                            Projects = new List<Project> 
                            {
                                new Project
                                {
                                    Name = "a.csproj",
                                    ProjectItems = new List<ProjectItem>
                                    {
                                        new ProjectItem { Name = "a.cs" },
                                        new ProjectItem { Name = "a\\b.cs" },
                                    }
                                },
                            }
                       },
                    }
                };

                EventLog = "This is the event log...";
            }
            else
            {
                Workspace = new Workspace();

                EventLog = "This is the event log...";
            }

            LoadUnloadWorkspace = new RelayCommand(
                async () =>
                {
                    await _workspace.LoadOrUnload();
                    CommandManager.InvalidateRequerySuggested();
                },
                _workspace.CanLoadOrUnload);
        }
    }
}