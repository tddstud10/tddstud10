using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public enum WorkspaceState
    {
        Unloaded,
        Loading,
        Loaded,
    }

    public class Workspace : ObservableObject
    {
        private List<Project> _projects = new List<Project>();

        public List<Project> Projects
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

        public async Task Disable()
        {
            State = WorkspaceState.Unloaded;
            await Task.Delay(1000);
        }

        public async Task LoadOrUnload()
        {
            if (State == WorkspaceState.Unloaded)
            {
                State = WorkspaceState.Loading;
                await Task.Delay(2000);
                State = WorkspaceState.Loaded;
            }
            else
            {
                State = WorkspaceState.Unloaded;
                await Task.Delay(2000);
            }
        }

        public bool CanLoadOrUnload()
        {
            return State != WorkspaceState.Loading;
        }
    }

    public class ProjectItem : ViewModelBase
    {
        private string _name = string.Empty;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }
    }

    public class Project : ViewModelBase
    {
        private List<Project> _projects = new List<Project>();

        public List<Project> Projects
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

        private List<ProjectItem> _projectItems = new List<ProjectItem>();

        public List<ProjectItem> ProjectItems
        {
            get { return _projectItems; }
            set
            {
                if (_projectItems == value)
                {
                    return;
                }

                _projectItems = value;
                RaisePropertyChanged(() => ProjectItems);
            }
        }

        public IEnumerable Children
        {
            get
            {
                return Enumerable.Empty<object>().Concat(Projects.Cast<object>().Concat(ProjectItems.Cast<object>()));
            }
        }

        public string Name { get; set; }
    }
}
