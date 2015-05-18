namespace R4nd0mApps.TddStud10.TestExecution.Adapters.Discovery

open R4nd0mApps.TddStud10.TestExecution
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type XUnitTestDiscoverer() = 
    let dc = TestPlatformExtensions.createDiscoveryContext()
    let ml = TestPlatformExtensions.createMessageLogger()
    let ds = TestPlatformExtensions.createDiscoverySink
    let testDiscovered = new Event<_>()
    member public t.TestDiscovered = testDiscovered.Publish
    member public t.DiscoverTests testAssembly = 
        let td = TestPlatformExtensions.loadTestAdapter() :?> ITestDiscoverer
        td.DiscoverTests([ testAssembly ], dc, ml, ds testDiscovered.Trigger)
