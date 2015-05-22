namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.Runtime.Serialization.Formatters.Binary
open System.IO
open System
open System.Collections.Generic

[<Serializable>]
type PerAssemblyTestIds = 
    inherit DataStoreBase<FilePath, ConcurrentBag<TestId>>
    
    new() = 
        { inherit DataStoreBase<_, _>() }
        then ()
    
    new(collection : IEnumerable<KeyValuePair<FilePath, ConcurrentBag<TestId>>>) = 
        { inherit DataStoreBase<_, _>(collection) }
        then ()
    
    static member public Deserialize path = DataStoreBase<_, _>.Deserialize<PerAssemblyTestIds> (path)
