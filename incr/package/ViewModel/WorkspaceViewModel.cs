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
    public enum SolutionState
    {
        Unloaded,
        Loading,
        Loaded,
        Unloading,
    }

    public class SolutionViewModel : ViewModelBase
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

        private SolutionState _state = SolutionState.Unloaded;

        public SolutionState State
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

        public void StartLoad(Solution workspace)
        {
            if (State != SolutionState.Unloaded)
            {
                return;
            }

            State = SolutionState.Loading;
        }

        public async Task FinishLoad(Solution workspace)
        {
            if (State != SolutionState.Loading)
            {
                return;
            }

            Projects = await CreateSolutionViewModelAsync(workspace);

            State = SolutionState.Loaded;
        }

        private static Task<List<ProjectViewModel>> CreateSolutionViewModelAsync(Solution solution)
        {
            return Task<List<ProjectViewModel>>.Run(
                () =>
                {
                    var projects = new List<ProjectViewModel>();
                    var dg = new AdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>();
                    solution.DependencyGraph.Clone(v => v, (e, s, d) => e, dg);

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
                        pvm.Children.AddRange(solution.Projects[v].ProjectReferences.Select(r => pvmMap[r]));
                        //pvm.Children.AddRange(solution.Projects[v].FileReferences.Select(f => new FileReferenceViewModel { FullName = f.Item }));
                        //pvm.Children.AddRange(solution.Projects[v].Items.Select(i => new ProjectItemViewModel { FullName = i.Item }));

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
            if (State != SolutionState.Loaded)
            {
                return;
            }

            State = SolutionState.Unloading;

            Projects.Clear();
            await Task.Delay(1000);
            State = SolutionState.Unloaded;
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
