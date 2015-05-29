namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerTestIdResults = 
    inherit DataStoreBase<TestId, ConcurrentBag<TestRunResult>>
    
    new() = 
        { inherit DataStoreBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStore.Serialize<PerTestIdResults> path t

    static member public Deserialize path = DataStore.Deserialize<PerTestIdResults> (path)
