namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open System
open R4nd0mApps.TddStud10.TestExecution
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type XUnitTestExecutor() = 
    let executorUri = new Uri("executor://xunit/VsTestRunner2")
    let rc = TestPlatformExtensions.createRunContext()
    let fh = TestPlatformExtensions.createFrameworkHandle
    let testExecuted = new Event<_>()
    member public t.TestExecuted = testExecuted.Publish
    // TODO: Change to FilePath
    member public t.ExecuteTests (testAssembly : string) = 
        let te = TestPlatformExtensions.loadTestAdapter() :?> ITestExecutor
        te.RunTests([testAssembly], rc, fh testExecuted.Trigger)
