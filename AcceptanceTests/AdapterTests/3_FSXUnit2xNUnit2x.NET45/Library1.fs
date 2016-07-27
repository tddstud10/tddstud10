namespace FSXUnit2xNUnit2x

open global.Xunit
open FsCheck.Xunit
open FsCheck
open FsUnit

type Class1() = 
    [<Fact>]
    let ``Reverse of reverse of a list is the original list``() =
        let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs
        Check.QuickThrowOnFailure revRevIsOrig

    let run c (s : string) = s.Split([|c|]).[0]

    [<Theory>]
    [<InlineData("an1 rest",           "an1")>]
    [<InlineData("?test bla",          "?test")>]
    let ``should parse symbol``(toParse:string, result:string) =
        Assert.Equal(result, run ' ' toParse)

//    [<Property>]
//    let ``Reverse of reverse of a list is the original list ``(xs:list<int>) =
//        List.rev(List.rev xs) = xs

    [<NUnit.Framework.Test>]
    member __.shouldSayHelloWithFsUnit () = 
        ("Hello " + "World!") |> should equal "Hello World!"
