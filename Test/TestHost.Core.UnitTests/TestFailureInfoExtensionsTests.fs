module R4nd0mApps.TddStud10.TestHost.TestFailureInfoExtensionsTests

open R4nd0mApps.TddStud10.Common.Domain
open System
open Xunit

let createFailedTR() = 
    { DisplayName = "???" 
      TestCase =
        { DtcId = Guid()
          FullyQualifiedName = "fqn"
          DisplayName = "???"
          Source = FilePath "a.dll"
          CodeFilePath = FilePath "???"
          LineNumber = DocumentCoordinate 0 }
      Outcome = TOFailed
      ErrorStackTrace = null
      ErrorMessage = null }

let ``If not Failure return empty seq - data`` : obj array seq = 
        [| TONone
           TONotFound
           TOPassed
           TOSkipped |]
        |> Seq.map (fun a -> [| box a |])

[<Theory>]
[<MemberData("If not Failure return empty seq - data")>]
let ``If not Failure return empty seq`` (o) = 
    let tr = { createFailedTR() with Outcome = o }
    let it = tr |> TestFailureInfoExtensions.create
    Assert.Empty(it)

[<Fact>]
let ``If Failure and ErrorStackTrace is null or empty, return empty seq``() = 
    let tr = createFailedTR()
    let it = tr |> TestFailureInfoExtensions.create
    Assert.Empty(it)

[<Fact>]
let ``If failure and ErrorStackTrace has 1 frame that cannot be parsed, return empty seq``() = 
    let tr = { createFailedTR() with
                ErrorMessage = "Test exception"
                ErrorStackTrace = "Anything that cannot be parsed as a stack frame" }
    let it = tr |> TestFailureInfoExtensions.create
    Assert.Empty(it)

[<Fact>]
let ``If failure and ErrorStackTrace has 1 frame that can be parsed, return that one with paths rebased``() = 
    let tr = { createFailedTR() with
                ErrorMessage = "Test exception"
                ErrorStackTrace = @"at NS.C.M() in d:\s\p\f.cs:line 15" }
    let it = tr |> TestFailureInfoExtensions.create
    
    let dl = 
        { document = FilePath @"d:\s\p\f.cs"
          line = DocumentCoordinate 15 }
    
    let tf = 
        { message = "Test exception"
          stack = [| ParsedFrame("NS.C.M()", dl) |] }
    
    Assert.Equal([| (dl, tf) |], it)

[<Fact>]
let ``If failure and ErrorStackTrace has 2 frames, return that parseable one with paths rebased``() = 
    let tr = { createFailedTR() with 
                ErrorMessage = null
                ErrorStackTrace = """    at NS.C.M(T p) in d:\s1\p2\f3.cs:line 1000
    at XNS.XC.XM()
    at YNS.YC.YM(YT yp, XT xp) in d:\s5\p6\f7.cpp:line 5000""" }
    let it = tr |> TestFailureInfoExtensions.create
    
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
                 UnparsedFrame("at XNS.XC.XM()")
                 ParsedFrame("YNS.YC.YM(YT yp, XT xp)", dl2) |] }
    
    Assert.Equal([| (dl1, tf)
                    (dl2, tf) |], it)
