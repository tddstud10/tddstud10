namespace R4nd0mApps.TddStud10.TestHost

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Concurrent

[<Serializable>]
type PerTestIdResults = 
    inherit DataStoreEntityBase<TestId, ConcurrentBag<TestResult>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerTestIdResults> path t
    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerTestIdResults>(path)
