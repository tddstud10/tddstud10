namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerDocumentLocationTestFailureInfo = 
    inherit DataStoreEntityBase<DocumentLocation, ConcurrentBag<TestFailureInfo>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerDocumentLocationTestFailureInfo> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerDocumentLocationTestFailureInfo> (path)
