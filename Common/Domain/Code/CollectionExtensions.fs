namespace R4nd0mApps.TddStud10.Common.Domain

[<RequireQualifiedAccess>]
module Dict = 
    open System.Collections.Generic
    
    let tryGetValue def f k (d : IDictionary<'TKey, 'TValue>) = 
        let found, trs = k |> d.TryGetValue
        if found && trs <> null then trs |> f
        else def
