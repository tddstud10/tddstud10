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
        Initializing,
        Loaded,
        Unloading,
    }

    public enum ProjectState
    {
        Loaded,
        CreatingSnapshot,
        ErrorCreatingSnapshot,
        Monitoring,
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

            Projects = CreateSolutionViewModelAsync(workspace);

            State = SolutionState.Initializing;
        }

        public void FinishLoad(Solution workspace)
        {
            if (State != SolutionState.Initializing)
            {
                return;
            }

            State = SolutionState.Loaded;
        }

        private List<ProjectViewModel> CreateSolutionViewModelAsync(Solution solution)
        {
            var projects = new List<ProjectViewModel>();
            var keys = from kvp in solution.DependencyMap
                       select kvp.Key;
            var dg = keys.ToAdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>(s => solution.DependencyMap[s].Select(t => new SEquatableEdge<ProjectId>(s, t)));

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
                pvm.FullName = v.UniqueName;
                pvm.ProjectId = v;
                pvm.Children.AddRange(solution.DependencyMap[v].Select(r => pvmMap[r]));
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

        public async Task Unload()
        {
            if (State != SolutionState.Loaded)
            {
                return;
            }

            State = SolutionState.Unloading;

            Projects = new List<ProjectViewModel>();
            await Task.Run(() => { });
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

        public ProjectId ProjectId { get; set; }

        private ProjectState _state = ProjectState.Loaded;

        public ProjectState State
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

        private List<string> _warnings = new List<string>();

        public List<string> Warnings
        {
            get
            {
                return _warnings;
            }
            set
            {
                Set(() => Warnings, ref _warnings, value);
            }
        }


        private List<string> _errors = new List<string>();

        public List<string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                Set(() => Errors, ref _errors, value);
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
