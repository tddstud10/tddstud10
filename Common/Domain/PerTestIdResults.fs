namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerTestIdResults = 
    inherit DataStoreEntityBase<TestId, ConcurrentBag<TestRunResult>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerTestIdResults> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerTestIdResults> (path)
