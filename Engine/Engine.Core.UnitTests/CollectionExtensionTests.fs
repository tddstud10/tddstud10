module R4nd0mApps.TddStud10.Engine.Core.CollectionExtensionTests

open Xunit
open R4nd0mApps.TddStud10.Common.Domain

[<Fact>]
let ``If key found, call f with key and return its output``() = 
    let x = [ ('a', "a") ] |> dict
    let v = x |> Dict.tryGetValue "default" (fun k -> string k) 'a'
    Assert.Equal("a", v)

[<Fact>]
let ``If key not found, return default value``() = 
    let x = [ ('a', "a") ] |> dict
    let v = x |> Dict.tryGetValue "default" (fun k -> string k) 'b'
    Assert.Equal("default", v)

[<Fact>]
let ``If key not found, but value is present and is null then return default value``() = 
    let x = [ ('a', null) ] |> dict
    let v = x |> Dict.tryGetValue "default" (fun k -> string k) 'a'
    Assert.Equal("default", v)
