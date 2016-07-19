namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open R4nd0mApps.TddStud10.TestExecution

type XUnitTestExecutor() = 
    let rc = TestPlatformExtensions.createRunContext()
    let fh = TestPlatformExtensions.createFrameworkHandle
    let testExecuted = new Event<_>()
    member public __.TestExecuted = testExecuted.Publish
    member public __.ExecuteTests(tes : ITestExecutor seq, tests : TestCase seq) = 
        tes
        |> Seq.map (fun te -> te.RunTests(tests, rc, fh testExecuted.Trigger))
        |> Seq.fold (fun _ -> id) ()
