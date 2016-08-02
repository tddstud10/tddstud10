namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.TestExecution
open Microsoft.FSharp.Control

type XUnitTestDiscoverer() = 
    let dc = TestPlatformExtensions.createDiscoveryContext()
    let ml = TestPlatformExtensions.createMessageLogger()
    let ds = TestPlatformExtensions.createDiscoverySink
    let filteredTest = new Event<_>()
    member public __.TestDiscovered = filteredTest.Publish
    member public __.DiscoverTests(tds : ITestDiscoverer seq, FilePath asm, ignoredTests : string[]) = 
        let testDiscovered = new Event<_>()
        let handler = fun obj (testCase: Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase) -> 
                            let exists = ignoredTests |> Array.exists (fun f -> testCase.FullyQualifiedName.StartsWith(f))
                            match exists with
                            | true -> ()
                            | false -> filteredTest.Trigger(testCase)
        testDiscovered.Publish.AddHandler(new Handler<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase>(handler))
        tds
        |> Seq.map (fun td -> td.DiscoverTests([ asm ], dc, ml, ds testDiscovered.Trigger))
        |> Seq.fold (fun _ -> id) ()
