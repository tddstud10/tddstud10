namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerAssemblySequencePointsCoverage = 
    inherit DataStoreEntityBase<AssemblyId, ConcurrentBag<SequencePointCoverage>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerAssemblySequencePointsCoverage> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerAssemblySequencePointsCoverage> (path)
