namespace R4nd0mApps.TddStud10.TestExecution.Adapters.Execution

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
    member public t.ExecuteTests(tests : TestCase seq) = 
        let te = TestPlatformExtensions.loadTestAdapter() :?> ITestExecutor
        te.RunTests(tests, rc, fh testExecuted.Trigger)
