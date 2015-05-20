namespace R4nd0mApps.TddStud10.Common.Domain

open System.Collections.Concurrent
open System.Runtime.Serialization.Formatters.Binary
open System.IO
open System
open System.Collections.Generic
open Microsoft.VisualStudio.TestPlatform.ObjectModel

[<Serializable>]
type PerTestIdResults = 
    inherit DataStoreBase<TestId, TestOutcome>
    
    new() = 
        { inherit DataStoreBase<_, _>() }
        then ()
    
    static member public Deserialize path = DataStoreBase<_, _>.Deserialize<PerTestIdResults> (path)
