using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
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
            EventLog.Insert(0, "...");

            LoadUnloadWorkspace = new RelayCommand(
                async () =>
                {
                    if (_solutionViewModel.State == SolutionState.Unloaded)
                    {
                        _pipeline = new IncrementalBuildPipeline(
                            FuncConvert.ToFSharpFunc<Solution>(
                                sln =>
                                {
                                    _eventLog.Insert(0, string.Format("Begin create snapshot {0}...", sln.Name));
                                    _solutionViewModel.StartLoad(sln);
                                }),
                            FuncConvert.ToFSharpFunc<ProjectId>(
                                pid =>
                                {
                                    _eventLog.Insert(0, string.Format("Begin create project snapshot {0}...", pid.Item));
                                }),
                            FuncConvert.ToFSharpFunc<ProjectId>(
                                pid =>
                                {
                                    _eventLog.Insert(0, string.Format("End create project snapshot {0}...", pid.Item));
                                }),
                            FuncConvert.ToFSharpFunc<Solution>(
                                async (sln) =>
                                {
                                    await _solutionViewModel.FinishLoad(sln);
                                    _eventLog.Insert(0, string.Format("End create snapshot {0}.", sln.Name));
                                }));

                        _pipeline.Trigger(Services.GetService<EnvDTE.DTE>().Solution);
                    }
                    else if (_solutionViewModel.State == SolutionState.Loaded)
                    {
                        ((IDisposable)_pipeline).Dispose();
                        await _solutionViewModel.Unload();
                        _eventLog.Clear();
                    }
                    else
                    {
                        // Do nothing!
                    }
                },
                () => _solutionViewModel.State != SolutionState.Loading);
        }
    }
}
