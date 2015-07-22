using GalaSoft.MvvmLight;
using QuickGraph;
using QuickGraph.Algorithms;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public enum WorkspaceState
    {
        Unloaded,
        Loading,
        Loaded,
        Unloading,
    }

    public class WorkspaceViewModel : ViewModelBase
    {
        private List<ProjectViewModel> _projects = new List<ProjectViewModel>();

        public List<ProjectViewModel> Projects
        {
            get { return _projects; }
            set
            {
                if (_projects == value)
                {
                    return;
                }

                _projects = value;
                RaisePropertyChanged(() => Projects);
            }
        }

        private WorkspaceState _state = WorkspaceState.Unloaded;

        public WorkspaceState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged(() => State);
            }
        }

        public async Task Load(Workspace workspace)
        {
            if (State != WorkspaceState.Unloaded)
            {
                return;
            }

            State = WorkspaceState.Loading;

            Projects = await CreateWorkspaceViewModelAsync(workspace);

            State = WorkspaceState.Loaded;
        }

        private static Task<List<ProjectViewModel>> CreateWorkspaceViewModelAsync(Workspace workspace)
        {
            return Task<List<ProjectViewModel>>.Run(
                () =>
                {
                    var projects = new List<ProjectViewModel>();
                    var dg = new AdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>();
                    workspace.DependencyGraph.Clone(v => v, (e, s, d) => e, dg);

                    var pvmMap = new Dictionary<ProjectId, ProjectViewModel>();
                    var sw = new Stopwatch();
                    sw.Start();
                    while (true)
                    {
                        var v = dg.Sinks().FirstOrDefault();
                        if (v == null)
                        {
                            break;
                        }

                        var pvm = new ProjectViewModel();
                        pvm.FullName = v.Item;
                        pvm.Children.AddRange(workspace.Projects[v].ProjectReferences.Select(r => pvmMap[r]));
                        pvm.Children.AddRange(workspace.Projects[v].FileReferences.Select(f => new FileReferenceViewModel { FullName = f.Item }));
                        pvm.Children.AddRange(workspace.Projects[v].Items.Select(i => new ProjectItemViewModel { FullName = i.Item }));

                        pvmMap[v] = pvm;

                        projects.Add(pvm);

                        dg.RemoveVertex(v);
                    }
                    sw.Stop();
                    Debug.WriteLine(">>>>>> TIME: {0}", sw.ElapsedMilliseconds);
                    return projects;
                }
            );
        }

        public async Task Unload()
        {
            if (State != WorkspaceState.Loaded)
            {
                return;
            }

            State = WorkspaceState.Unloading;

            Projects.Clear();
            await Task.Delay(1000);
            State = WorkspaceState.Unloaded;
        }
    }

    public class ProjectViewModel : ViewModelBase
    {
        private List<object> _children = new List<object>();

        public List<object> Children
        {
            get { return _children; }
            set
            {
                if (_children == value)
                {
                    return;
                }

                _children = value;
                RaisePropertyChanged(() => Children);
            }
        }

        private string _fullName;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName == value)
                {
                    return;
                }

                _fullName = value;
                RaisePropertyChanged(() => FullName);
            }
        }
    }

    public abstract class ProjectChildBaseViewModel : ViewModelBase
    {
        private string _fullName;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName == value)
                {
                    return;
                }

                _fullName = value;
                RaisePropertyChanged(() => FullName);
            }
        }
    }

    public class ProjectItemViewModel : ProjectChildBaseViewModel
    {
    }

    public class ProjectReferenceViewModel : ProjectChildBaseViewModel
    {

    }

    public class FileReferenceViewModel : ProjectChildBaseViewModel
    {

    }
}
