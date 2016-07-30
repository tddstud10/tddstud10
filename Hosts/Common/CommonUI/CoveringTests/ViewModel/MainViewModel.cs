using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using R4nd0mApps.TddStud10.Common.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        // NOTE: Only a layout debugging aid. Can be removed once UI is stable.
        private static bool debugLayout = false;

        private bool _popupStaysOpen = false;

        public bool PopupStaysOpen
        {
            get { return _popupStaysOpen; }
            set
            {
                _popupStaysOpen = value;
                RaisePropertyChanged(() => PopupStaysOpen);
            }
        }

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

        private HostIdeActions _hostActions;

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

        public MainViewModel(GlyphInfo glyphInfo, HostIdeActions hostActions)
            : this()
        {
            _glyphInfo = glyphInfo;

            _hostActions = hostActions;

            ShowPopupCommand = new RelayCommand(ShowPopup);
        }

        private void ShowPopup()
        {
            InitializePopup();

            PopupVisible =
                CoveringTests != null
                && CoveringTests.Any()
                && _hostActions != null
                && !_hostActions.IdeInDebugMode.Invoke(null);
        }

        private void InitializePopup()
        {
            if (CoveringTests != null)
            {
                return;
            }

            var vms =
                _hostActions
                .GetCoveringTestResults.Invoke(null)
                .Select(tr =>
                    new CoveringTestViewModel
                    {
                        ShowGridLines = debugLayout,
                        TestCase = tr,
                        TestPassed = tr.Item2.Outcome.Equals(DTestOutcome.TOPassed) ? true : tr.Item2.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                        Id = tr.Item2.TestCase.DtcId,
                        FullyQualifiedName = tr.Item2.TestCase.FullyQualifiedName,
                        DisplayName = tr.Item2.TestCase.DisplayName,
                        ErrorMessage = FormatErrorField("Error Message", tr.Item2.ErrorMessage),
                        ErrorStackTrace = FormatErrorField("Stack Trace", tr.Item2.ErrorStackTrace),
                        GotoTestCommand = new RelayCommand<Tuple<SequencePoint, DTestResult>>(GotoTest),
                        DebugTestCommand = new RelayCommand<Tuple<SequencePoint, DTestResult>>(DebugTest),
                        RunTestCommand = new RelayCommand<Tuple<SequencePoint, DTestResult>>(RunTest),
                    });

            CoveringTests = new ObservableCollection<CoveringTestViewModel>(vms);
        }

        private static string FormatErrorField(string desc, string value)
        {
            return string.IsNullOrEmpty(value) ? "" : string.Format("{0}: \n    {1}", desc, value.Replace("\n", "\n    "));
        }

        private void GotoTest(Tuple<SequencePoint, DTestResult> tr)
        {
            PopupVisible = false;
            _hostActions.GotoTest.Invoke(tr);
        }

        private void DebugTest(Tuple<SequencePoint, DTestResult> tr)
        {
            PopupVisible = false;
            _hostActions.DebugTest.Invoke(tr);
        }

        private void RunTest(Tuple<SequencePoint, DTestResult> tr)
        {
            PopupVisible = false;
            _hostActions.RunTest.Invoke(tr);
        }
    }
}