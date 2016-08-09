module R4nd0mApps.TddStud10.Common.Domain.FilePathTests

open System
open System.Collections
open System.Collections.Generic
open R4nd0mApps.TddStud10.Common.Domain
open Xunit

let inline (~~) s = FilePath s

[<Theory>]
[<InlineData("abc", "aBc", true)>]
[<InlineData("abc", "abc", true)>]
[<InlineData("abc", "xyz", false)>]
let ``FilePath - HashCode tests`` s1 s2 same = Assert.Equal((~~s1).GetHashCode() = (~~s2).GetHashCode(), same)

[<Theory>]
[<InlineData("abc", "abc")>]
[<InlineData("aBc", "aBc")>]
let ``FilePath - ToString tests`` s1 s2 = Assert.Equal((~~s1).ToString(), s2)

let ``Equals Test Data`` = 
    [ ~~"abc", box ~~"abc", true
      ~~"abc", box ~~"aBc", true
      ~~"abc", box "xyz", false
      ~~"abc", box 1, false
      ~~"abc", null, false
      ~~"abc", box ~~"xyz", false ]
    |> Seq.map (fun (a, b, c) -> 
           [| box a
              b
              box c |])

[<Theory>]
[<MemberData("Equals Test Data")>]
let ``FilePath - Equals tests`` (p1 : FilePath) p2 same = Assert.Equal(p1.Equals(p2), same)

let ``IComparable Test Data`` = 
    [ 0, ~~"abc", box ~~"abc"
      0, ~~"abc", box ~~"aBc"
      -1, ~~"abc", box ~~"Bbc"
      1, ~~"bbc", box ~~"Abc"
      1, ~~"abc", box "xyz"
      1, ~~"abc", box 1
      1, ~~"abc", null ]
    |> Seq.map (fun (a, b, c) -> 
           [| box a
              box b
              c |])

[<Theory>]
[<MemberData("IComparable Test Data")>]
let ``FilePath - IComparable tests`` i (p1 : IComparable) p2 = Assert.Equal(i, p1.CompareTo(p2))

[<Fact>]
let ``FilePath - Set insert and retrieve``() = 
    let s = Set.empty.Add(~~"Abc")
    Assert.True(s.Contains(~~"aBC"))
    let s = new HashSet<FilePath>()
    s.Add(~~"aBC") |> ignore
    Assert.True(s.Contains(~~"aBC"))

[<Fact>]
let ``FilePath - Map insert and retrieve``() = 
    let m = Map.empty.Add(~~"Abc", 1)
    Assert.Equal(1, m.[~~"aBC"])
    let m = new Dictionary<FilePath, int>()
    m.[~~"aBC"] <- 1
    Assert.Equal(1, m.[~~"AbC"])
