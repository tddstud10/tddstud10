namespace R4nd0mApps.TddStud10.TestExecution.Adapters

open R4nd0mApps.TddStud10.TestExecution
open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type XUnitTestDiscoverer() = 
    let dc = TestPlatformExtensions.createDiscoveryContext()
    let ml = TestPlatformExtensions.createMessageLogger()
    let ds = TestPlatformExtensions.createDiscoverySink
    let testDiscovered = new Event<_>()
    member public __.TestDiscovered = testDiscovered.Publish
    member public __.DiscoverTests(FilePath asm) = 
        let td = TestPlatformExtensions.loadTestAdapter() :?> ITestDiscoverer
        td.DiscoverTests([ asm ], dc, ml, ds testDiscovered.Trigger)
