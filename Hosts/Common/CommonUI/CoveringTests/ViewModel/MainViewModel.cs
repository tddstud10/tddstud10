using GalaSoft.MvvmLight;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public class MainViewModel : ViewModelBase
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

        public MainViewModel()
        {
            _coveringTests = new ObservableCollection<CoveringTestViewModel>(
                new List<DTestResult> {
                    new DTestResult("First test name",
                        new DTestCase(
                            "test #1 fqn",
                            "First test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(10)),
                        DTestOutcome.TOPassed,
                        null, null),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                    new DTestResult("Second test name",
                        new DTestCase(
                            "test #2 fqn",
                            "Second test name",
                            FilePath.NewFilePath(@"c:\a.dll"),
                            FilePath.NewFilePath(@"c:\a.cs"),
                            DocumentCoordinate.NewDocumentCoordinate(20)),
                        DTestOutcome.TOFailed,
                        "Stack trace for second test", "failure in second test"),
                }.Select(it => new CoveringTestViewModel
                {
                    TestResult = it,
                    TestPassed = it.Outcome == DTestOutcome.TOPassed ? true : it.Outcome == DTestOutcome.TOFailed ? (bool?)false : null,
                    DisplayName = it.DisplayName,
                    ErrorMessage = it.ErrorMessage,
                    ErrorStackTrace = it.ErrorStackTrace,
                }));
        }
    }
}
