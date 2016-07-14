namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open R4nd0mApps.TddStud10.TestExecution

type XUnitTestExecutor() = 
    let rc = TestPlatformExtensions.createRunContext()
    let fh = TestPlatformExtensions.createFrameworkHandle
    let testExecuted = new Event<_>()
    member public __.TestExecuted = testExecuted.Publish
    member public __.ExecuteTests(binDir, tests : TestCase seq) = 
        let te = binDir |> TestPlatformExtensions.loadTestAdapter :?> ITestExecutor
        te.RunTests(tests, rc, fh testExecuted.Trigger)
