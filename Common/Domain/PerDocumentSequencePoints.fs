namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.Runtime.Serialization.Formatters.Binary
open System.IO
open System
open System.Collections.Generic

[<Serializable>]
type PerDocumentSequencePoints = 
    inherit DataStoreBase<FilePath, ConcurrentBag<SequencePoint>>
    
    new() = 
        { inherit DataStoreBase<_, _>() }
        then ()
    
    member public t.Serialize path = DataStore.Serialize<PerDocumentSequencePoints> path t

    static member public Deserialize path = DataStore.Deserialize<PerDocumentSequencePoints> (path)
