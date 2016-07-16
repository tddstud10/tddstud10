namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.TestExecution

type XUnitTestDiscoverer() = 
    let dc = TestPlatformExtensions.createDiscoveryContext()
    let ml = TestPlatformExtensions.createMessageLogger()
    let ds = TestPlatformExtensions.createDiscoverySink
    let testDiscovered = new Event<_>()
    member public __.TestDiscovered = testDiscovered.Publish
    member public __.DiscoverTests(tds : ITestDiscoverer seq, FilePath asm) = 
        tds
        |> Seq.map (fun td -> td.DiscoverTests([ asm ], dc, ml, ds testDiscovered.Trigger))
        |> Seq.reduce (fun _ -> id)
