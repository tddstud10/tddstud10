namespace R4nd0mApps.TddStud10.Common.Domain

type IDataStore = 
    abstract RunStartParams : RunStartParams option
    abstract TestCasesUpdated : IEvent<PerDocumentLocationDTestCases>
    abstract SequencePointsUpdated : IEvent<PerDocumentSequencePoints>
    abstract TestResultsUpdated : IEvent<PerTestIdDResults>
    abstract TestFailureInfoUpdated : IEvent<PerDocumentLocationTestFailureInfo>
    abstract CoverageInfoUpdated : IEvent<PerSequencePointIdTestRunId>
    abstract UpdateRunStartParams : RunStartParams -> unit
    abstract UpdateData : RunData -> unit
    abstract ResetData : unit -> unit
    abstract FindTest : DocumentLocation -> seq<DTestCase>
    abstract GetSequencePointsForFile : FilePath -> seq<SequencePoint>
    abstract FindTestFailureInfo : DocumentLocation -> seq<TestFailureInfo>
    abstract GetRunIdsForTestsCoveringSequencePointId : SequencePointId -> seq<TestRunId>
    abstract GetResultsForTestId : TestId -> seq<DTestResult>
