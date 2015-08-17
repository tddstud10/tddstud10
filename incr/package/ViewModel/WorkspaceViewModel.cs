using GalaSoft.MvvmLight;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow.ViewModel
{
    public enum SolutionState
    {
        Unloaded,
        Loading,
        Loaded,
    }

    public enum ProjectOperationState
    {
        Running,
        Failed,
        Succeeded,
    }

    public class SolutionViewModel : ViewModelBase
    {
        private ObservableCollection<ProjectViewModel> _projects = new ObservableCollection<ProjectViewModel>();
        public ObservableCollection<ProjectViewModel> Projects
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

        private string _currentOperation = "Solution not loaded...";
        public string CurrentOperation
        {
            get { return _currentOperation; }

            set
            {
                if (_currentOperation == value)
                {
                    return;
                }

                _currentOperation = value;
                RaisePropertyChanged(() => CurrentOperation);
            }
        }

        private bool _currentOperationInProgress = false;
        public bool CurrentOperationInProgress
        {
            get { return _currentOperationInProgress; }

            set
            {
                if (_currentOperationInProgress == value)
                {
                    return;
                }

                _currentOperationInProgress = value;
                RaisePropertyChanged(() => CurrentOperationInProgress);
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

        public void FinishLoad(Solution workspace)
        {
            if (State != SolutionState.Loading)
            {
                return;
            }

            State = SolutionState.Loaded;
        }

        public async Task Unload()
        {
            if (State != SolutionState.Loaded)
            {
                return;
            }

            Projects.Clear();
            await Task.Run(() => { });

            CurrentOperation = "Solution unloaded";
            CurrentOperationInProgress = false;
            State = SolutionState.Unloaded;
        }
    }

    public class ProjectViewModel : ViewModelBase
    {
        private ObservableCollection<object> _children = new ObservableCollection<object>();
        public ObservableCollection<object> Children
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

        private string _fullName = string.Empty;
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

        private ProjectOperationState _operationState = ProjectOperationState.Succeeded;
        public ProjectOperationState OperationState
        {
            get { return _operationState; }
            set
            {
                if (_operationState == value)
                {
                    return;
                }

                _operationState = value;
                RaisePropertyChanged(() => OperationState);
            }
        }

        public ObservableCollection<string> Issues { get; set; }
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
