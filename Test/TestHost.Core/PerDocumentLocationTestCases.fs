namespace R4nd0mApps.TddStud10.TestHost

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Concurrent

[<Serializable>]
type PerDocumentLocationTestCases = 
    inherit DataStoreEntityBase<DocumentLocation, ConcurrentBag<TestCase>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerDocumentLocationTestCases> path t
    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerDocumentLocationTestCases>(path)
