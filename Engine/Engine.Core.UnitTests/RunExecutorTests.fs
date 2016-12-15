module R4nd0mApps.TddStud10.Engine.Core.RunExecutorTests

open Xunit
open System
open R4nd0mApps.TddStud10
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestDoubles
open R4nd0mApps.TddStud10.Engine.TestFramework
open System.IO

let cfg = EngineConfig()
cfg.SnapShotRoot <- "%WINDIR%"
cfg.AdditionalMSBuildProperties <- [|"AdditionalMSBuildProperties"|]
cfg.IgnoredTests <- "IgnoredTests"
let now = DateTime.Now
let host = new TestHost(Int32.MaxValue)
let ex = (new InvalidOperationException("A mock method threw")) :> Exception
let slnFile = ~~"c:\\folder\\file.sln"
let stubRsp = RunStartParams.Create cfg now slnFile

let createSteps n = 
    [| for _ in 1..n do
           yield (new StepFunc()) |]

let toRSF : StepFunc array -> RunStep array = Array.map RS
let createHandlers() = (new CallSpy<RunStartParams>(), new CallSpy<Exception>(), new CallSpy<RunStartParams>())
let createRE h ss f = RunExecutor.Create h (ss |> toRSF) f

let createRE2 h ss _ (sh : CallSpy<_>, erh : CallSpy<_>, eh : CallSpy<_>) = 
    let re = createRE h ss id
    re.RunStarting.Add(sh.Func >> ignore)
    re.OnRunError.Add(erh.Func >> ignore)
    re.RunEnded.Add(eh.Func >> ignore)
    re

let startRE (re : RunExecutor) = re.Start(cfg, now, slnFile)

let areRdsSimillar rd1 rd2 = 
    match rd1 with
    | None -> false
    | Some rd1 -> rd1.Solution.Path = rd2.Solution.Path

[<Fact>]
let ``Executor initialized RunData``() = 
    let re = RunExecutor.Create host [||] id
    let rsp, err = startRE re
    
    let getFileName (FilePath p) = 
        p
        |> Path.GetFileName
        |> FilePath
    
    let actual = { rsp with TestHostPath = (rsp.TestHostPath |> getFileName) }
    
    let ssRoot = Environment.ExpandEnvironmentVariables(cfg.SnapShotRoot)
    let expected = 
        { SnapShotRoot = ~~ssRoot
          StartTime = now
          TestHostPath = ~~(sprintf "TddStud10.TestHost%s.exe" Constants.ProductVariant)
          Solution = 
            { Path = slnFile
              SnapshotPath = ~~(ssRoot + @"\folder\file.sln")
              BuildRoot = ~~(ssRoot + @"\folder\out") }
          IgnoredTests = cfg.IgnoredTests
          AdditionalMSBuildProperties = cfg.AdditionalMSBuildProperties
          DataFiles = 
              { SequencePointStore = ~~(ssRoot + @"\folder\out\Z_sequencePointStore.xml")
                CoverageSessionStore = ~~(ssRoot + @"\folder\out\Z_coverageresults.xml")
                TestResultsStore = ~~(ssRoot + @"\folder\out\Z_testresults.xml")
                DiscoveredUnitTestsStore = ~~(ssRoot + @"\folder\out\Z_discoveredUnitTests.xml")
                DiscoveredUnitDTestsStore = ~~(ssRoot + @"\folder\out\Z_discoveredUnitDTests.xml")
                TestFailureInfoStore = ~~(ssRoot + @"\folder\out\Z_testFailureInfo.xml") } }
    Assert.Equal(expected, actual)
    Assert.Equal(err, None)

[<Fact>]
let ``Executor calls all steps sequentially - verify inputs and outputs``() = 
    let ss = createSteps 2
    let re = createRE host ss id
    let it = startRE re
    Assert.True(None <> ss.[0].CalledWith && None <> ss.[0].ReturningWith)
    Assert.True(None <> ss.[1].CalledWith && None <> ss.[1].ReturningWith)
    let shared (h, s, _, e) = h, s, e
    Assert.Equal(ss.[0].CalledWith |> Option.map shared, ss.[1].CalledWith |> Option.map shared)
    let unique (_, _, i, _) = i
    Assert.NotEqual(ss.[0].CalledWith |> Option.map unique, ss.[1].CalledWith |> Option.map unique)
    Assert.Equal((stubRsp, None), it)

[<Fact>]
let ``Executor allows injection of behavior for each step``() = 
    let ss = createSteps 1
    let injSpy = new CallSpy<RunStepFunc>()
    let re = createRE host ss injSpy.Func
    startRE re |> ignore
    Assert.True(injSpy.Called, "Injector should have been executed")

[<Fact>]
let ``Executor raises starting and ended events only``() = 
    let ss = createSteps 1
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 host ss id (sh, erh, eh)
    let rsp, err = startRE re
    Assert.True(sh.Called && not erh.Called && eh.Called, "Only start and end handlers should have been called")
    Assert.True(areRdsSimillar sh.CalledWith rsp)
    Assert.True((eh.CalledWith |> Option.map (fun rsp -> rsp.GetHashCode())) = (Some(rsp.GetHashCode())))
    Assert.Equal(err, None)

[<Fact>]
let ``Exception in handler - Errored and Ended are raised even if Starting throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunStartParams>(Throws(ex)), new CallSpy<Exception>(), new CallSpy<RunStartParams>()
    let re = createRE2 (new TestHost(1)) ss id (sh, erh, eh)
    startRE re |> ignore
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Errored and Ended should have been called")

[<Fact>]
let ``Exception in handler - Starting and Ended are raised even if Errored throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunStartParams>(), new CallSpy<Exception>(Throws(ex)), new CallSpy<RunStartParams>()
    let re = createRE2 (new TestHost(1)) ss id (sh, erh, eh)
    startRE re |> ignore
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Started and Ended should have been called")

[<Fact>]
let ``Exception in handler - Starting and Errored are raised even if Ended throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunStartParams>(), new CallSpy<Exception>(), new CallSpy<RunStartParams>(Throws(ex))
    let re = createRE2 (new TestHost(1)) ss id (sh, erh, eh)
    startRE re |> ignore
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Started and Errored should have been called")

[<Fact>]
let ``Cancellation - Executor raises all 3 events and stops execution``() = 
    let ss = createSteps 3
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 (new TestHost(1)) ss id (sh, erh, eh)
    let rsp, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True(areRdsSimillar sh.CalledWith rsp)
    Assert.True((eh.CalledWith |> Option.map (fun rsp -> rsp.GetHashCode())) = (Some(rsp.GetHashCode())))
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(err <> None && err = erh.CalledWith, "Error returned should also have been passed to error handler")

[<Fact>]
let ``Step fails - Random Exception - Executor raises all 3 events and stops execution``() = 
    let ss = Array.append [| new StepFunc(Throws(ex)) |] (createSteps 2)
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 host ss id (sh, erh, eh)
    let _, err = startRE re
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True
        (err = (Some ex) && erh.CalledWith = (Some ex), "Error returned should also have been passed to error handler")

[<Fact>]
let ``Step fails - RunStepFailedException - Executor raises all 3 events, stops execution, passes output rss to RunEnded event``() = 
    let rss = 
        { status = Failed
          addendum = FreeFormatData "There has been a failure"
          runData = NoData }
    
    let rsfe = RunStepFailedException rss
    let ss = Array.append [| new StepFunc(Throws(rsfe)) |] (createSteps 2)
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 host ss id (sh, erh, eh)
    startRE re |> ignore
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.Equal(eh.CalledWith, Some stubRsp)
    Assert.Equal(erh.CalledWith, Some rsfe)
