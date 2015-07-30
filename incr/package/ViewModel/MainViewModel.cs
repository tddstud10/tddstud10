using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

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
                        Children = new List<object>
                        {
                            new ProjectViewModel { FullName = "Bello" }
                        }
                    });
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
                                            _ =>
                                            {
                                                _solutionViewModel.StartLoad(sln);
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<ProjectId>(
                                pid =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            _ =>
                                            {
                                                var x = _solutionViewModel.Projects.Find(p => p.ProjectId == pid);
                                                if (x != null)
                                                {
                                                    x.State = ProjectState.CreatingSnapshot;
                                                }
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<Tuple<ProjectId, ProjectLoadResult>>(
                                args =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            _ =>
                                            {
                                                var project = _solutionViewModel.Projects.Find(p => p.ProjectId == args.Item1);
                                                if (project != null)
                                                {
                                                    project.Errors.AddRange(args.Item2.Warnings);
                                                    project.Errors.AddRange(args.Item2.Errors);
                                                    if (args.Item2.Status)
                                                    {
                                                        project.State = ProjectState.Monitoring;
                                                    }
                                                    else
                                                    {
                                                        project.State = ProjectState.ErrorCreatingSnapshot;
                                                    }
                                                }
                                            }), null);
                                }),
                            FuncConvert.ToFSharpFunc<Solution>(
                                sln =>
                                {
                                    _syncContext.Send(
                                        new SendOrPostCallback(
                                            _ =>
                                            {
                                                _solutionViewModel.FinishLoad(sln);
                                            }), null);
                                }));

                        _pipeline.Trigger(Services.GetService<EnvDTE.DTE>().Solution);
                    }
                    else if (_solutionViewModel.State == SolutionState.Loaded)
                    {
                        ((IDisposable)_pipeline).Dispose();
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

    public class TreeViewSelectedItemBlendBehavior : Behavior<TreeView>
    {
        //dependency property
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object),
            typeof(TreeViewSelectedItemBlendBehavior),
            new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true });

        //property wrapper
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}
