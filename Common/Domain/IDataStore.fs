namespace R4nd0mApps.TddStud10.Common.Domain

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type IDataStore = 
    abstract RunStartParams : RunStartParams option
    abstract TestCasesUpdated : IEvent<PerDocumentLocationTestCases>
    abstract SequencePointsUpdated : IEvent<PerDocumentSequencePoints>
    abstract TestResultsUpdated : IEvent<PerTestIdResults>
    abstract TestFailureInfoUpdated : IEvent<PerDocumentLocationTestFailureInfo>
    abstract CoverageInfoUpdated : IEvent<PerSequencePointIdTestRunId>
    abstract UpdateRunStartParams : RunStartParams -> unit
    abstract UpdateData : RunData -> unit
    abstract FindTest : DocumentLocation -> seq<TestCase>
    abstract GetSequencePointsForFile : FilePath -> seq<SequencePoint>
    abstract FindTestFailureInfo : DocumentLocation -> seq<TestFailureInfo>
    abstract GetRunIdsForTestsCoveringSequencePointId : SequencePointId -> seq<TestRunId>
    abstract GetResultsForTestId : TestId -> seq<TestResult>
