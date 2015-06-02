namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System

[<Serializable>]
type PerDocumentSequencePoints = 
    inherit DataStoreEntityBase<FilePath, ConcurrentBag<SequencePoint>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerDocumentSequencePoints> path t

    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerDocumentSequencePoints> (path)
