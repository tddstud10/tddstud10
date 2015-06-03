namespace R4nd0mApps.TddStud10.Common.Domain

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type IDataStore = 
    abstract TestCasesUpdated : IEvent<PerAssemblyTestCases>
    abstract UpdateData : RunStepResult -> unit
    abstract FindTestByDocumentAndLineNumber : FilePath -> DocumentCoordinate -> TestCase option
