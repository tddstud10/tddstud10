namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.Runtime.Serialization.Formatters.Binary
open System.IO
open System
open System.Collections.Generic
open Microsoft.VisualStudio.TestPlatform.ObjectModel

[<Serializable>]
type PerAssemblyTestCases = 
    inherit DataStoreBase<FilePath, ConcurrentBag<TestCase>>
    
    new() = 
        { inherit DataStoreBase<_, _>() }
        then ()
    
    new(collection : IEnumerable<KeyValuePair<_, _>>) = 
        { inherit DataStoreBase<_, _>(collection) }
        then ()
    
    member public t.Serialize path = DataStore.Serialize<PerAssemblyTestCases> path t
    
    static member public Deserialize path = DataStore.Deserialize<PerAssemblyTestCases> (path)
