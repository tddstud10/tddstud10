namespace BUnit.TestAdapter

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type VsTestDiscoverer() = 
    let rt = Runtime.Class1()
    interface ITestDiscoverer with
        member __.DiscoverTests(_ : System.Collections.Generic.IEnumerable<string>, _ : IDiscoveryContext, 
                                _ : Logging.IMessageLogger, _ : ITestCaseDiscoverySink) : unit = 
            failwith "Not implemented yet"
