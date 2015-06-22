namespace R4nd0mApps.TddStud10.Common.Domain

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type IDataStore = 
    abstract RunStartParams : RunStartParams option
    abstract TestCasesUpdated : IEvent<PerAssemblyTestCases>
    abstract SequencePointsUpdated : IEvent<PerDocumentSequencePoints>
    abstract TestResultsUpdated : IEvent<PerTestIdResults>
    abstract TestFailureInfoUpdated : IEvent<PerDocumentLocationTestFailureInfo>
    abstract CoverageInfoUpdated : IEvent<PerAssemblySequencePointsCoverage>
    abstract UpdateRunStartParams : RunStartParams -> unit
    abstract UpdateData : RunData -> unit
    abstract FindTest : FilePath -> FilePath -> DocumentCoordinate -> TestCase option
    abstract FindTest2 : FilePath -> DocumentCoordinate -> TestCase seq
    abstract GetSequencePointsForFile : FilePath -> SequencePoint seq
    abstract FindTestFailureInfo : DocumentLocation -> TestFailureInfo seq
