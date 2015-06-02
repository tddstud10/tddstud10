namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System
open System.Collections.Generic
open Microsoft.VisualStudio.TestPlatform.ObjectModel

[<Serializable>]
type PerAssemblyTestCases = 
    inherit DataStoreEntityBase<FilePath, ConcurrentBag<TestCase>>
    
    new() = 
        { inherit DataStoreEntityBase<_, _>() }
        then ()
    
    new(collection : IEnumerable<KeyValuePair<_, _>>) = 
        { inherit DataStoreEntityBase<_, _>(collection) }
        then ()
    
    member public t.Serialize path = DataStoreEntityExtensions.Serialize<PerAssemblyTestCases> path t
    
    static member public Deserialize path = DataStoreEntityExtensions.Deserialize<PerAssemblyTestCases> (path)
