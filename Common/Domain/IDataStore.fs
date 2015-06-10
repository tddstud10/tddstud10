namespace R4nd0mApps.TddStud10.Common.Domain

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type IDataStore = 
    // Provide the other 3 data members from RunData - change it to Option
    abstract SolutionBuildRoot : FilePath with get
    abstract TestCasesUpdated : IEvent<PerAssemblyTestCases>
    abstract UpdateData : RunStepResult -> unit
    abstract FindTestByDocumentAndLineNumber : FilePath -> DocumentCoordinate -> TestCase option
