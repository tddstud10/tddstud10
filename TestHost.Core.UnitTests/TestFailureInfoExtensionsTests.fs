module R4nd0mApps.TddStud10.TestHost.TestFailureInfoExtensionsTests

open Xunit
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System
open R4nd0mApps.TddStud10.Common.Domain

let stubRsp = RunStartParamsExtensions.create DateTime.UtcNow (FilePath @"d:\s\s.sln")

let createFailedTR() = 
    let tr = TestResult(TestCase("fqn", Uri("a://b/c"), "a.dll"))
    tr.Outcome <- TestOutcome.Failed
    tr

[<Theory>]
[<InlineData(TestOutcome.None)>]
[<InlineData(TestOutcome.NotFound)>]
[<InlineData(TestOutcome.Passed)>]
[<InlineData(TestOutcome.Skipped)>]
let ``If not Failure return empty seq`` o = 
    let tr = createFailedTR()
    tr.Outcome <- o
    let it = tr |> TestFailureInfoExtensions.create stubRsp
    Assert.Empty(it)

[<Fact>]
let ``If Failure and ErrorStackTrace is null or empty, return empty seq``() = 
    let tr = createFailedTR()
    let it = tr |> TestFailureInfoExtensions.create stubRsp
    Assert.Empty(it)

[<Fact>]
let ``If failure and ErrorStackTrace has 1 frame that cannot be parsed, return empty seq``() = 
    let tr = createFailedTR()
    tr.ErrorMessage <- "Test exception"
    tr.ErrorStackTrace <- "Anything that cannot be parsed as a stack frame"
    let it = tr |> TestFailureInfoExtensions.create stubRsp
    Assert.Empty(it)

[<Fact>]
let ``If failure and ErrorStackTrace has 1 frame that can be parsed, return that one with paths rebased``() = 
    let tr = createFailedTR()
    tr.ErrorMessage <- "Test exception"
    tr.ErrorStackTrace <- @"at NS.C.M(T p) in d:\tddstud10\s\p\f.cs:line 15"
    let it = tr |> TestFailureInfoExtensions.create stubRsp
    
    let dl = 
        { document = FilePath @"d:\s\p\f.cs"
          line = DocumentCoordinate 15 }
    
    let tf = 
        { message = "Test exception"
          stack = [| ParsedFrame("NS.C.M(T p)", dl) |] }
    
    Assert.Equal([| (dl, tf) |], it)

[<Fact>]
let ``If failure and ErrorStackTrace has 2 frames, return that parseable one with paths rebased``() = 
    let tr = createFailedTR()
    tr.ErrorMessage <- null
    tr.ErrorStackTrace <- """at NS.C.M(T p) in d:\tddstud10\s1\p2\f3.cs:line 1000
at XNS.XC.XM(XT xp)
at YNS.YC.YM(YT yp) in d:\tddstud10\s5\p6\f7.cpp:line 5000"""
    let it = tr |> TestFailureInfoExtensions.create stubRsp
    
    let dl1 = 
        { document = FilePath @"d:\s1\p2\f3.cs"
          line = DocumentCoordinate 1000 }
    
    let dl2 = 
        { document = FilePath @"d:\s5\p6\f7.cpp"
          line = DocumentCoordinate 5000 }
    
    let tf = 
        { message = ""
          stack = 
              [| ParsedFrame("NS.C.M(T p)", dl1)
                 UnparsedFrame("at XNS.XC.XM(XT xp)")
                 ParsedFrame("YNS.YC.YM(YT yp)", dl2) |] }
    
    Assert.Equal([| (dl1, tf)
                    (dl2, tf) |], it)
