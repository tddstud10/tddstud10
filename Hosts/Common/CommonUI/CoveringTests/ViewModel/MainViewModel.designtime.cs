using GalaSoft.MvvmLight;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private static readonly IEnumerable<DTestResult> DesignTimeData = new List<DTestResult> {
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
                };
    }
}