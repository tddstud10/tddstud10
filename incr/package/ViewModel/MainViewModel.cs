using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private SynchronizationContext _syncContext = SynchronizationContext.Current;
        private IncrementalBuildPipeline _pipeline;

        private SolutionViewModel _solutionViewModel = new SolutionViewModel();
        public SolutionViewModel SolutionViewModel
        {
            get { return _solutionViewModel; }
        }

        private ProjectViewModel _selectedProject = null;
        public ProjectViewModel SelectedProject
        {
            get
            {
                return _selectedProject;
            }

            set
            {
                if (_selectedProject == value)
                {
                    return;
                }

                _selectedProject = value;
                RaisePropertyChanged(() => SelectedProject);
            }
        }

        public RelayCommand LoadUnloadWorkspace { get; set; }

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                SolutionViewModel.Projects.Add(
                    new ProjectViewModel
                    {
                        FullName = "Jello",
                        OperationState = ProjectOperationState.Running,
                        Children = new ObservableCollection<object>
                        {
                            new ProjectViewModel 
                            { 
                                FullName = "Bello",
                                OperationState = ProjectOperationState.Running,
                            }
                        }
                    });
            }

            LoadUnloadWorkspace = new RelayCommand(
                async () =>
                {
                    if (SolutionViewModel.State == SolutionState.Unloaded)
                    {
                        _pipeline = new IncrementalBuildPipeline(
                            new IncrementalBuildPipelineEventsHandlers
                            {
                                LoadStarting =
                                    FuncConvert.ToFSharpFunc<Solution>(
                                        sln =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        SolutionViewModel.StartLoad(sln);
                                                        SolutionViewModel.CurrentOperation = "Loading Solution...";
                                                        SolutionViewModel.CurrentOperationInProgress = true;
                                                    }), null);
                                        }),
                                ProjectLoadStarting =
                                    FuncConvert.ToFSharpFunc<ProjectId>(
                                        pid =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = new ProjectViewModel
                                                        {
                                                            ProjectId = pid,
                                                            FullName = pid.UniqueName + " (Loading...)",
                                                            OperationState = ProjectOperationState.Succeeded
                                                        };
                                                        SolutionViewModel.Projects.Add(project);
                                                    }), null);
                                        }),
                                ProjectLoadFinished =
                                    FuncConvert.ToFSharpFunc<Tuple<ProjectId, OperationResult<Project, IEnumerable<string>>>>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(args.Item1));
                                                        if (project != null)
                                                        {
                                                            project.FullName = project.ProjectId.UniqueName;
                                                            if (args.Item2.IsSuccess)
                                                            {
                                                                project.OperationState = ProjectOperationState.Succeeded;
                                                                var res = args.Item2 as OperationResult<Project, IEnumerable<string>>.Success;
                                                                foreach (var pid in res.Item.ProjectReferences)
                                                                {
                                                                    var pref = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(pid));
                                                                    if (pref != null)
                                                                    {
                                                                        project.Children.Add(pref);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                project.OperationState = ProjectOperationState.Failed;
                                                                var res = args.Item2 as OperationResult<Project, IEnumerable<string>>.Failure;
                                                                res.Item.Aggregate(project.Issues, (acc, e) => { acc.Add(e); return acc; });
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Logger.I.LogError("Project {0} not found in the view model.", args.Item1);
                                                        }
                                                    }), null);
                                        }),
                                LoadFinished =
                                    FuncConvert.ToFSharpFunc<Tuple<Solution, OperationResult<Unit, Unit>>>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        SolutionViewModel.CurrentOperation = "Solution Loaded...";
                                                        SolutionViewModel.CurrentOperationInProgress = false;
                                                        SolutionViewModel.FinishLoad(args.Item1);
                                                    }), null);
                                        }),
                                SyncAndBuildStarting =
                                    FuncConvert.ToFSharpFunc<Solution>(
                                        sln =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        SolutionViewModel.CurrentOperation = "Build and Sync Starting...";
                                                        SolutionViewModel.CurrentOperationInProgress = true;
                                                    }), null);
                                        }),
                                ProjectSyncStarting =
                                    FuncConvert.ToFSharpFunc<Project>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(args.Id));
                                                        if (project != null)
                                                        {
                                                            project.FullName = project.ProjectId.UniqueName + " (syncing...)";
                                                            project.OperationState = ProjectOperationState.Running;
                                                        }
                                                    }), null);
                                        }),
                                ProjectSyncFinished =
                                    FuncConvert.ToFSharpFunc<Tuple<Project, OperationResult<Tuple<FilePath, IEnumerable<string>>, IEnumerable<string>>>>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(args.Item1.Id));
                                                        if (project != null)
                                                        {
                                                            project.FullName = project.ProjectId.UniqueName;
                                                            if (args.Item2.IsSuccess)
                                                            {
                                                                project.OperationState = ProjectOperationState.Succeeded;
                                                            }
                                                            else
                                                            {
                                                                var res = args.Item2 as OperationResult<Tuple<FilePath, IEnumerable<string>>, IEnumerable<string>>.Failure;
                                                                project.OperationState = ProjectOperationState.Failed;
                                                                res.Item.Aggregate(project.Issues, (acc, e) => { acc.Add(e); return acc; });
                                                            }
                                                        }
                                                    }), null);
                                        }),
                                ProjectBuildStarting =
                                    FuncConvert.ToFSharpFunc<Project>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(args.Id));
                                                        if (project != null)
                                                        {
                                                            project.FullName = project.ProjectId.UniqueName + " (building...)";
                                                            project.OperationState = ProjectOperationState.Running;
                                                        }
                                                    }), null);
                                        }),
                                ProjectBuildFinished =
                                    FuncConvert.ToFSharpFunc<Tuple<Project, OperationResult<Tuple<IEnumerable<FilePath>, IEnumerable<string>>, IEnumerable<string>>>>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        var project = SolutionViewModel.Projects.FirstOrDefault(p => p.ProjectId.Equals(args.Item1.Id));
                                                        if (project != null)
                                                        {
                                                            project.FullName = project.ProjectId.UniqueName;
                                                            if (args.Item2.IsSuccess)
                                                            {
                                                                project.OperationState = ProjectOperationState.Succeeded;
                                                                var res = args.Item2 as OperationResult<Tuple<IEnumerable<FilePath>, IEnumerable<string>>, IEnumerable<string>>.Success;
                                                                res.Item.Item2.Aggregate(project.Issues, (acc, e) => { acc.Add(e); return acc; });
                                                            }
                                                            else
                                                            {
                                                                project.OperationState = ProjectOperationState.Failed;
                                                                var res = args.Item2 as OperationResult<Tuple<IEnumerable<FilePath>, IEnumerable<string>>, IEnumerable<string>>.Failure;
                                                                res.Item.Aggregate(project.Issues, (acc, e) => { acc.Add(e); return acc; });
                                                            }
                                                        }
                                                    }), null);
                                        }),
                                SyncAndBuildFinished =
                                    FuncConvert.ToFSharpFunc<Tuple<Solution, OperationResult<Unit, Unit>>>(
                                        args =>
                                        {
                                            _syncContext.Send(
                                                new SendOrPostCallback(
                                                    _ =>
                                                    {
                                                        SolutionViewModel.CurrentOperation = "Build and Sync Finished...";
                                                        SolutionViewModel.CurrentOperationInProgress = false;
                                                    }), null);
                                        }),
                            });

                        _pipeline.Trigger(Services.GetService<EnvDTE.DTE>().Solution);
                    }
                    else if (SolutionViewModel.State == SolutionState.Loaded)
                    {
                        _pipeline.Unload();
                        ((IDisposable)_pipeline).Dispose();
                        await SolutionViewModel.Unload();
                    }
                    else
                    {
                        Logger.I.LogError("Should not have come here as the command should have been disabled!");
                    }
                },
                () =>
                    SolutionViewModel.State == SolutionState.Loaded || SolutionViewModel.State == SolutionState.Unloaded);
        }
    }
}
