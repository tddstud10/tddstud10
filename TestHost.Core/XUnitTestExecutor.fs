namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open System
open R4nd0mApps.TddStud10.TestExecution
open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open Microsoft.VisualStudio.TestPlatform.ObjectModel

(*
- Perf
  v Are the tests getting discovered again?
  - Parallelism - 2 x number of processors
  - DRY Removal of assembly filter logic
  - Make that parallel
- Serialization of tests
  v Change unit tests
  v Change serializer
  v Change data model
    v rename PerAssemblyTestCases
    v TestRunResult
    v Do we still need TestId
  v remove t.ExecuteTests(FilePath asm)
- Fix solution items issue
 *)
type XUnitTestExecutor() = 
    let executorUri = new Uri("executor://xunit/VsTestRunner2")
    let rc = TestPlatformExtensions.createRunContext()
    let fh = TestPlatformExtensions.createFrameworkHandle
    let testExecuted = new Event<_>()
    member public t.TestExecuted = testExecuted.Publish
    member public t.ExecuteTests(tests : TestCase seq) = 
        let te = TestPlatformExtensions.loadTestAdapter() :?> ITestExecutor
        te.RunTests(tests, rc, fh testExecuted.Trigger)
