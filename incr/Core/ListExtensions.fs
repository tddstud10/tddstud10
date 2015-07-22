[<AutoOpen>]
module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ListExtensions

open System.Collections

module List = 
    let fromUntypedEnumerable<'T> (enumerable : IEnumerable) = 
        if obj.ReferenceEquals(enumerable, null) then []
        else 
            [ for e in enumerable -> e :?> 'T ]
