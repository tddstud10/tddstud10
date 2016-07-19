namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerDocumentLocationDTestCases = 
    inherit DataStoreEntityBase<DocumentLocation, ConcurrentBag<DTestCase>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()

    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerDocumentLocationDTestCases> path t
    
    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerDocumentLocationDTestCases> (path)
