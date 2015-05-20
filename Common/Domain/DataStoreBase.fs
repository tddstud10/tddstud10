namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.Runtime.Serialization.Formatters.Binary
open System.IO
open System
open System.Collections.Generic

[<Serializable>]
type DataStoreBase<'TKey, 'TValue> = 
    inherit ConcurrentDictionary<'TKey, 'TValue>
    
    new() = 
        { inherit ConcurrentDictionary<'TKey, 'TValue>() }
        then ()
    
    new(collection : IEnumerable<KeyValuePair<'TKey, 'TValue>>) = 
        { inherit ConcurrentDictionary<'TKey, 'TValue>(collection) }
        then ()
    
    member public t.Serialize path = 
        let fmter = new BinaryFormatter()
        use s = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
        fmter.Serialize(s, t)
    
    static member public Deserialize<'T> path = 
        let fmter = new BinaryFormatter()
        use s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
        fmter.Deserialize(s) :?> 'T
