namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.TestExecution
open Microsoft.FSharp.Control

type XUnitTestDiscoverer() = 
    let dc = TestPlatformExtensions.createDiscoveryContext()
    let ml = TestPlatformExtensions.createMessageLogger()
    let ds = TestPlatformExtensions.createDiscoverySink
    let filteredTest = new Event<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase>()
    let testDiscovered = new Event<_>()
    let isValidTest = fun ignoredTests (testCase : Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase) -> 
                                            not(
                                                ignoredTests 
                                                |> Array.exists (fun (ignoredTestPattern) -> testCase.FullyQualifiedName.StartsWith(ignoredTestPattern))
                                               )
    member public __.TestDiscovered = filteredTest.Publish
    member public __.DiscoverTests(tds : ITestDiscoverer seq, FilePath asm, ignoredTests : string[]) = 
        testDiscovered.Publish |> Event.filter(fun testCase -> isValidTest ignoredTests testCase)
                               |> Event.add (fun testCase -> filteredTest.Trigger(testCase))
        tds
        |> Seq.map (fun td -> td.DiscoverTests([ asm ], dc, ml, ds testDiscovered.Trigger))
        |> Seq.fold (fun _ -> id) ()
