namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerTestIdDResults = 
    inherit DataStoreEntityBase<TestId, ConcurrentBag<DTestResult>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerTestIdDResults> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerTestIdDResults> (path)
