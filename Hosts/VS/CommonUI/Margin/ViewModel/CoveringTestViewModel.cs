using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using R4nd0mApps.TddStud10.Common.Domain;
using System;

namespace R4nd0mApps.TddStud10.Hosts.Common.Margin.ViewModel
{
    public class CoveringTestViewModel : ViewModelBase
    {
        // NOTE: Only a layout debugging aid. Can be removed once UI is stable.
        private bool _showGridLines = false;

        public bool ShowGridLines
        {
            get { return _showGridLines; }
            set
            {
                _showGridLines = value;
                RaisePropertyChanged(() => _showGridLines);
            }
        }

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

        private Guid _id;

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        private string _fullyQualifiedName;

        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName; }
            set
            {
                _fullyQualifiedName = value;
                RaisePropertyChanged(() => FullyQualifiedName);
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

        private string _errorStackTrace;

        public string ErrorStackTrace
        {
            get { return _errorStackTrace; }
            set
            {
                _errorStackTrace = value;
                RaisePropertyChanged(() => ErrorStackTrace);
            }
        }

        public Tuple<SequencePoint, DTestResult> TestCase { get; set; }

        public RelayCommand ShowDetailsCommand { get; private set; }

        public RelayCommand<Tuple<SequencePoint, DTestResult>> GotoTestCommand { get; set; }

        public RelayCommand<Tuple<SequencePoint, DTestResult>> DebugTestCommand { get; set; }

        public RelayCommand<Tuple<SequencePoint, DTestResult>> RunTestCommand { get; set; }

        public CoveringTestViewModel()
        {
            ShowDetailsCommand = new RelayCommand(() => { DetailsVisible = !DetailsVisible; });
        }
    }
}
