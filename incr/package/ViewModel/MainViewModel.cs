using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private SynchronizationContext _syncContext = SynchronizationContext.Current;
        private IncrementalBuildPipeline _pipeline;
        private ObservableCollection<string> _eventLog = new ObservableCollection<string>();

        public ObservableCollection<string> EventLog
        {
            get { return _eventLog; }
        }

        private SolutionViewModel _solutionViewModel = new SolutionViewModel();

        public SolutionViewModel SolutionViewModel
        {
            get { return _solutionViewModel; }
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
                        Children = new List<object>
                        {
                            new ProjectViewModel { FullName = "Bello" }
                        }
                    });
                EventLog.Insert(0, "...");
            }

            LoadUnloadWorkspace = new RelayCommand(
                async () =>
                {
                    if (_solutionViewModel.State == SolutionState.Unloaded)
                    {
                        _pipeline = new IncrementalBuildPipeline(
                            FuncConvert.ToFSharpFunc<Solution>(
                                sln =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            async _ =>
                                            {
                                                _eventLog.Insert(0, string.Format("Begin create snapshot {0}...", sln.Name));
                                                await _solutionViewModel.StartLoad(sln);
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<ProjectId>(
                                pid =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            _ =>
                                            {
                                                _eventLog.Insert(0, string.Format("Begin create project snapshot {0}...", pid.Item));
                                                var x = _solutionViewModel.Projects.Find(p => p.ProjectId == pid);
                                                if (x != null)
                                                {
                                                    x.State = ProjectState.CreatingSnapshot;
                                                }
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<ProjectId>(
                                pid =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            _ =>
                                            {
                                                _eventLog.Insert(0, string.Format("End create project snapshot {0}.", pid.Item));
                                                var x = _solutionViewModel.Projects.Find(p => p.ProjectId == pid);
                                                if (x != null)
                                                {
                                                    x.State = ProjectState.Monitoring;
                                                }
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<Solution>(
                                sln =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            async _ =>
                                            {
                                                await _solutionViewModel.FinishLoad(sln);
                                                _eventLog.Insert(0, string.Format("End create snapshot {0}.", sln.Name));
                                            }), null);
                                }));

                        _pipeline.Trigger(Services.GetService<EnvDTE.DTE>().Solution);
                    }
                    else if (_solutionViewModel.State == SolutionState.Loaded)
                    {
                        ((IDisposable)_pipeline).Dispose();
                        _eventLog.Clear();
                        await _solutionViewModel.Unload();
                    }
                    else
                    {
                        // Should not have come here as the command should have been disabled!
                    }
                },
                () => _solutionViewModel.State == SolutionState.Loaded || _solutionViewModel.State == SolutionState.Unloaded);
        }
    }
}
