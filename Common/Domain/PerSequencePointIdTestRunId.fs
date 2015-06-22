namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerSequencePointIdTestRunId = 
    inherit DataStoreEntityBase<SequencePointId, ConcurrentBag<TestRunId>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerSequencePointIdTestRunId> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerSequencePointIdTestRunId> (path)
