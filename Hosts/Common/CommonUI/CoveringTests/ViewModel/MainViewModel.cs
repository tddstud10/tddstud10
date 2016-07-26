using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private bool _popupVisible = false;

        public bool PopupVisible
        {
            get { return _popupVisible; }
            set
            {
                _popupVisible = value;
                RaisePropertyChanged(() => PopupVisible);
            }
        }

        private readonly HostIdeActions _hostActions;

        private GlyphInfo _glyphInfo;

        public GlyphInfo GlyphInfo
        {
            get { return _glyphInfo; }
            set
            {
                _glyphInfo = value;
                RaisePropertyChanged(() => GlyphInfo);
            }
        }

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

        public RelayCommand ShowPopupCommand { get; set; }

        [PreferredConstructor]
        public MainViewModel()
        {
            ShowPopupCommand = new RelayCommand(
                () =>
                {
                    PopupVisible = CoveringTests != null && CoveringTests.Any();
                });
        }

        public MainViewModel(HostIdeActions hostActions, GlyphInfo glyphInfo, IEnumerable<DTestResult> coveringTestResults)
            : this()
        {
            _hostActions = hostActions;

            GlyphInfo = glyphInfo;

            _coveringTests = new ObservableCollection<CoveringTestViewModel>(
                coveringTestResults.Select(it => new CoveringTestViewModel
                {
                    TestResult = it,
                    TestPassed = it.Outcome.Equals(DTestOutcome.TOPassed) ? true : it.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                    DisplayName = it.TestCase.FullyQualifiedName,
                    ErrorMessage = it.ErrorMessage,
                    ErrorStackTrace = it.ErrorStackTrace,
                    GotoTestCommand = new RelayCommand<DTestCase>(GotoTest),
                    DebugTestCommand = new RelayCommand<DTestCase>(DebugTest),
                    RunTestCommand = new RelayCommand<DTestCase>(RunTest),
                }));
        }

        private void GotoTest(DTestCase testCase)
        {
            PopupVisible = false;
            _hostActions.GotoTest(testCase);
        }

        private void DebugTest(DTestCase testCase)
        {
            PopupVisible = false;
            _hostActions.DebugTest(testCase);
        }

        private void RunTest(DTestCase testCase)
        {
            PopupVisible = false;
            _hostActions.RunTest(testCase);
        }
    }
}