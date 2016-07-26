using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.FSharp.Core;
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

        public RelayCommand InitializeViewModelCommand { get; set; }

        public RelayCommand ShowPopupCommand { get; set; }

        [PreferredConstructor]
        public MainViewModel()
        {
        }

        public MainViewModel(GlyphInfo glyphInfo, FSharpFunc<Unit, Tuple<HostIdeActions, IEnumerable<Tuple<SequencePoint, IEnumerable<DTestResult>>>>> getVmInfo)
            : this()
        {
            _glyphInfo = glyphInfo;

            InitializeViewModel(getVmInfo);

            // TODO: Figure out a way to make this work so we can delay load the VM data
            //InitializeViewModelCommand = new RelayCommand(
            //    () =>
            //    {
            //        if (CoveringTests == null)
            //        {
            //            Dispatcher.CurrentDispatcher.InvokeAsync(() => InitializeViewModel(getVmInfo), DispatcherPriority.DataBind);
            //        }
            //    });

            ShowPopupCommand = new RelayCommand(
                () =>
                {
                    PopupVisible =
                        CoveringTests != null
                        && CoveringTests.Any()
                        && _hostActions != null
                        && !_hostActions.IdeInDebugMode.Invoke(null);
                });
        }

        private void InitializeViewModel(FSharpFunc<Unit, Tuple<HostIdeActions, IEnumerable<Tuple<SequencePoint, IEnumerable<DTestResult>>>>> getVmInfo)
        {
            var vmInfo = getVmInfo.Invoke(null);

            _hostActions = vmInfo.Item1;

            var vms =
                vmInfo.Item2
                .Select(
                    it =>
                        it.Item2
                        .Select(tr =>
                            new CoveringTestViewModel
                            {
                                TestCase = new Tuple<SequencePoint, DTestCase>(it.Item1, tr.TestCase),
                                TestPassed = tr.Outcome.Equals(DTestOutcome.TOPassed) ? true : tr.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                                Id = tr.TestCase.DtcId,
                                FullyQualifiedName = tr.TestCase.FullyQualifiedName,
                                DisplayName = tr.TestCase.DisplayName,
                                ErrorMessage = tr.ErrorMessage,
                                ErrorStackTrace = tr.ErrorStackTrace,
                                GotoTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(GotoTest),
                                DebugTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(DebugTest),
                                RunTestCommand = new RelayCommand<Tuple<SequencePoint, DTestCase>>(RunTest),
                            }))
                .SelectMany(_ => _)
                .GroupBy(vm => vm.Id)
                .Select(vmg => vmg.First());

            _coveringTests = new ObservableCollection<CoveringTestViewModel>(vms);
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