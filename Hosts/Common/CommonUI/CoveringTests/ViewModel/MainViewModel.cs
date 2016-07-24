using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private ObservableCollection<CoveringTestViewModel> _coveringTests;

        public ObservableCollection<CoveringTestViewModel> CoveringTests
        {
            get { return _coveringTests; }
            set
            {
                _coveringTests = value;
                RaisePropertyChanged(() => CoveringTests);
            }
        }

        [PreferredConstructor]
        public MainViewModel()
            : this(DesignTimeData)
        {
        }

        public MainViewModel(IEnumerable<DTestResult> coveringTestResults)
        {
            _coveringTests = new ObservableCollection<CoveringTestViewModel>(
                coveringTestResults.Select(it => new CoveringTestViewModel
                {
                    TestResult = it,
                    TestPassed = it.Outcome.Equals(DTestOutcome.TOPassed) ? true : it.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                    DisplayName = it.TestCase.FullyQualifiedName,
                    ErrorMessage = it.ErrorMessage,
                    ErrorStackTrace = it.ErrorStackTrace,
                }));
        }
    }
}