using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using R4nd0mApps.TddStud10.Common.Domain;
using System;
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
        }

        public MainViewModel(HostIdeActions hostActions, GlyphInfo glyphInfo, IEnumerable<Tuple<SequencePoint, IEnumerable<DTestResult>>> coveringTestResults)
            : this()
        {
            _hostActions = hostActions;

            _glyphInfo = glyphInfo;

            var vms =
                coveringTestResults
                .Select(
                    it =>
                        it.Item2.Select(tr =>
                        new CoveringTestViewModel
                        {
                            TestCase = new Tuple<SequencePoint, DTestCase>(it.Item1, tr.TestCase),
                            TestPassed = tr.Outcome.Equals(DTestOutcome.TOPassed) ? true : tr.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                            DisplayName = tr.TestCase.FullyQualifiedName,
                            ErrorMessage = tr.ErrorMessage,
                            ErrorStackTrace = tr.ErrorStackTrace,
                            GotoTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(GotoTest),
                            DebugTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(DebugTest),
                            RunTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(RunTest),
                        }))
                .SelectMany(_ => _);

            _coveringTests = new ObservableCollection<CoveringTestViewModel>(vms);

            ShowPopupCommand = new RelayCommand(
                () =>
                {
                    PopupVisible =
                        CoveringTests != null
                        && CoveringTests.Any()
                        && !_hostActions.IdeInDebugMode.Invoke(null);
                });
        }

        private void GotoTest(Tuple<SequencePoint, DTestCase> testCase)
        {
            PopupVisible = false;
            _hostActions.GotoTest.Invoke(testCase);
        }

        private void DebugTest(Tuple<SequencePoint, DTestCase> testCase)
        {
            PopupVisible = false;
            _hostActions.DebugTest.Invoke(testCase);
        }

        private void RunTest(Tuple<SequencePoint, DTestCase> testCase)
        {
            PopupVisible = false;
            _hostActions.RunTest.Invoke(testCase);
        }
    }
}