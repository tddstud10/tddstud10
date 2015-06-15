namespace R4nd0mApps.TddStud10.Common.Domain

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type IDataStore = 
    abstract RunStartParams : RunStartParams option with get
    abstract TestCasesUpdated : IEvent<PerAssemblyTestCases>
    abstract SequencePointsUpdated : IEvent<PerDocumentSequencePoints>
    abstract TestResultsUpdated : IEvent<PerTestIdResults>
    [<CLIEvent>]
    abstract CoverageInfoUpdated : IEvent<PerAssemblySequencePointsCoverage>
    abstract UpdateRunStartParams : RunStartParams -> unit
    abstract UpdateData : RunData -> unit
    abstract FindTest : FilePath -> FilePath -> DocumentCoordinate -> TestCase option
    abstract FindTest2 : FilePath -> DocumentCoordinate -> TestCase seq
    abstract GetAllFiles : unit -> FilePath seq
    abstract GetAllSequencePoints : unit -> SequencePoint seq
    abstract GetUnitTestsCoveringSequencePoint : SequencePoint -> TestRunId seq
    abstract GetTestResults : TestId -> TestRunResult seq

