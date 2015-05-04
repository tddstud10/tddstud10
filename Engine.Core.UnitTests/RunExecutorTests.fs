module R4nd0mApps.TddStud10.Engine.Core.RunExecutorTests

open Xunit
open System
open R4nd0mApps.TddStud10.TestHost
open R4nd0mApps.TddStud10.Engine.TestDoubles
open R4nd0mApps.TddStud10.Engine.TestFramework

let inline (~~) s = FilePath s
let now = DateTime.Now
let host = new TestHost(Int32.MaxValue)

let createSteps n = 
    [| for i in 1..n do
           yield (new StepFunc()) |]

let toRSF : StepFunc array -> RunStep array = Array.map (fun s -> RS s)
let createHandlers() = (new CallSpy<unit>(), new CallSpy<Exception>(), new CallSpy<unit>())
let createRE ss f = new RunExecutor(ss |> toRSF, f)

let createRE2 ss f (sh : CallSpy<unit>, erh : CallSpy<Exception>, eh : CallSpy<unit>) = 
    let re = createRE ss (fun f -> f)
    re.RunStarting.Add(sh.Func)
    re.RunErrored.Add(erh.Func >> ignore)
    re.RunEnded.Add(eh.Func)
    re

let startRE (re : RunExecutor) = re.Start(host, now, ~~"c:\\folder\\file.sln") |> Async.RunSynchronously
let startRE2 (re : RunExecutor) h = re.Start(h, now, ~~"c:\\folder\\file.sln") |> Async.RunSynchronously

[<Fact>]
let ``Executor initialized RunData``() = 
    let re = new RunExecutor([||], fun f -> f)
    let rd, err = startRE re
    Assert.Equal(rd.startTime, now)
    Assert.Equal(rd.solutionPath, ~~"c:\\folder\\file.sln")
    Assert.Equal(rd.solutionSnapshotPath, ~~"d:\\tddstud10\\folder\\file.sln")
    Assert.Equal(rd.solutionBuildRoot, ~~"d:\\tddstud10\\folder.out")
    Assert.Equal(err, None)

[<Fact>]
let ``Executor initialized RunData - sln without parent folder``() = 
    let re = new RunExecutor([||], fun f -> f)
    let rd, err = re.Start(host, now, ~~"c:\\file.sln") |> Async.RunSynchronously
    Assert.Equal(rd.startTime, now)
    Assert.Equal(rd.solutionPath, ~~"c:\\file.sln")
    Assert.Equal(rd.solutionSnapshotPath, ~~"d:\\tddstud10\\file\\file.sln")
    Assert.Equal(rd.solutionBuildRoot, ~~"d:\\tddstud10\\file.out")
    Assert.Equal(err, None)

[<Fact>]
let ``Executor calls all steps sequentially and relays rundata``() = 
    let ss = createSteps 2
    let re = createRE ss (fun f -> f)
    let rd, err = startRE re
    Assert.True
        (ss.[0].CalledWith <> ss.[0].ReturningWith && ss.[0].ReturningWith = ss.[1].CalledWith, 
         "step 2 is not called with the output of step 1")
    Assert.True
        (ss.[1].CalledWith <> ss.[1].ReturningWith && ss.[1].ReturningWith = Some(rd.GetHashCode()), 
         "returned value is not the output of step 2")
    Assert.True((err = None), "Error should have passed")

[<Fact>]
let ``Executor allows injection of behavior for each step``() = 
    let ss = createSteps 1
    let injSpy = new CallSpy<RunStepFunc>()
    let re = createRE ss injSpy.Func
    let rd, err = startRE re
    Assert.True(injSpy.Called, "Injector should have been executed")

[<Fact>]
let ``Executor raises starting and ended events only``() = 
    let ss = createSteps 1
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 ss (fun f -> f) (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && not erh.Called && eh.Called, "Only start and end handlers should have been called")
    Assert.Equal(err, None)

[<Fact>]
let ``Cancellation - Executor raises all 3 events and stops execution``() = 
    let ss = createSteps 3
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 ss (fun f -> f) (sh, erh, eh)
    let rd, err = startRE2 re (new TestHost(1))
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(err <> None && err = erh.CalledWith, "Error returned should also have been passed to error handler")

[<Fact>]
let ``Step throws - Executor raises all 3 events and stops execution``() = 
    let ss = Array.append [| new StepFunc(true) |] (createSteps 2)
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 ss (fun f -> f) (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(err <> None && err = erh.CalledWith, "Error returned should also have been passed to error handler")

// Next
// - Not in engine
//   - stepStarted
//   - stepEnded
(*
testdoubles.runexector methods are not called + methods inside an async block
unable to understand red due to build failure - needed to build
unable to understand exception caused and where - needed to look into the test failures
test execution seems to halt initially - not as fast as the rest of teh steps!!!
debugging really really needed
*)
