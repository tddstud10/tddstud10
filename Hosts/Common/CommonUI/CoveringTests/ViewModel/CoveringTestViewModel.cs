using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Windows;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public class CoveringTestViewModel : ViewModelBase
    {
        private bool _expanded = false;

        public bool DetailsVisible
        {
            get { return _expanded; }
            set
            {
                _expanded = value;
                RaisePropertyChanged(() => DetailsVisible);
            }
        }

        private bool? _testPassed;

        public bool? TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                RaisePropertyChanged(() => TestPassed);
            }
        }

        private string _displayName;

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                RaisePropertyChanged(() => DisplayName);
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }

        private string _stackTrace;

        public string ErrorStackTrace
        {
            get { return _stackTrace; }
            set
            {
                _stackTrace = value;
                RaisePropertyChanged(() => ErrorStackTrace);
            }
        }

        public DTestResult TestResult { get; set; }

        public RelayCommand ShowDetailsCommand { get; set; }

        public RelayCommand GotoTestCommand { get; set; }

        public RelayCommand StartWithDebuggingCommand { get; set; }

        public RelayCommand StartWithoutDebuggingCommand { get; set; }

        public CoveringTestViewModel()
        {
            ShowDetailsCommand = new RelayCommand(() => { DetailsVisible = !DetailsVisible; });
            GotoTestCommand = new RelayCommand(() => { MessageBox.Show("Goto test..."); });
            StartWithDebuggingCommand = new RelayCommand(() => { MessageBox.Show("Start with debugging..."); });
            StartWithoutDebuggingCommand = new RelayCommand(() => { MessageBox.Show("Start without debugging..."); });
        }
    }
}
