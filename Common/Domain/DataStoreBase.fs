namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.IO
open System.Collections.Generic
open System.Runtime.Serialization

type DataStore = 
    
    static member public Serialize<'T> (FilePath path) (t : 'T) = 
        let serializer = new DataContractSerializer(typeof<'T>)
        use stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
        serializer.WriteObject(stream, t)
    
    static member public Deserialize<'T>(FilePath path) = 
        let serializer = new DataContractSerializer(typeof<'T>)
        use stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
        serializer.ReadObject(stream) :?> 'T

type DataStoreBase<'TKey, 'TValue> = 
    inherit ConcurrentDictionary<'TKey, 'TValue>
    
    new() = 
        { inherit ConcurrentDictionary<'TKey, 'TValue>() }
        then ()
    
    new(collection : IEnumerable<KeyValuePair<'TKey, 'TValue>>) = 
        { inherit ConcurrentDictionary<'TKey, 'TValue>(collection) }
        then ()
