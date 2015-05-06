module R4nd0mApps.TddStud10.Engine.Core.RunExecutorTests

open Xunit
open System
open R4nd0mApps.TddStud10.TestHost
open R4nd0mApps.TddStud10.Engine.TestDoubles
open R4nd0mApps.TddStud10.Engine.TestFramework

let inline (~~) s = FilePath s
let now = DateTime.Now
let host = new TestHost(Int32.MaxValue)
let I = fun f -> f

let createSteps n = 
    [| for i in 1..n do
           yield (new StepFunc()) |]

let toRSF : StepFunc array -> RunStep array = Array.map (fun s -> RS s)
let createHandlers() = (new CallSpy<RunData>(), new CallSpy<Exception>(), new CallSpy<RunData>())
let createRE h ss f = RunExecutor.Create h (ss |> toRSF) f

let createRE2 h ss f (sh : CallSpy<RunData>, erh : CallSpy<Exception>, eh : CallSpy<RunData>) = 
    let re = createRE h ss I
    re.RunStarting.Add(sh.Func >> ignore)
    re.OnRunError.Add(erh.Func >> ignore)
    re.RunEnded.Add(eh.Func >> ignore)
    re

let startRE (re : RunExecutor) = re.Start(now, ~~"c:\\folder\\file.sln")

let areRdsSimillar rd1 rd2 = 
    match rd1 with
    | None -> false
    | Some rd1 -> rd1.solutionPath = rd2.solutionPath

[<Fact>]
let ``Executor initialized RunData``() = 
    let re = RunExecutor.Create host [||] I
    let rd, err = startRE re
    Assert.Equal(rd.startTime, now)
    Assert.Equal(rd.solutionPath, ~~"c:\\folder\\file.sln")
    Assert.Equal(rd.solutionSnapshotPath, ~~"d:\\tddstud10\\folder\\file.sln")
    Assert.Equal(rd.solutionBuildRoot, ~~"d:\\tddstud10\\folder.out")
    Assert.Equal(err, None)

[<Fact>]
let ``Executor initialized RunData - sln without parent folder``() = 
    let re = RunExecutor.Create host [||] I
    let rd, err = re.Start(now, ~~"c:\\file.sln")
    Assert.Equal(rd.startTime, now)
    Assert.Equal(rd.solutionPath, ~~"c:\\file.sln")
    Assert.Equal(rd.solutionSnapshotPath, ~~"d:\\tddstud10\\file\\file.sln")
    Assert.Equal(rd.solutionBuildRoot, ~~"d:\\tddstud10\\file.out")
    Assert.Equal(err, None)

[<Fact>]
let ``Executor calls all steps sequentially and relays rundata``() = 
    let ss = createSteps 2
    let re = createRE host ss I
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
    let re = createRE host ss injSpy.Func
    let rd, err = startRE re
    Assert.True(injSpy.Called, "Injector should have been executed")

[<Fact>]
let ``Executor raises starting and ended events only``() = 
    let ss = createSteps 1
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 host ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && not erh.Called && eh.Called, "Only start and end handlers should have been called")
    Assert.True
        ((areRdsSimillar sh.CalledWith rd) && (areRdsSimillar eh.CalledWith rd), 
         "Start and end events should be called with rundata")
    Assert.Equal(err, None)

[<Fact>]
let ``Exception in handler - Errored and Ended are raised even if Starting throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunData>(Throws), new CallSpy<Exception>(), new CallSpy<RunData>()
    let re = createRE2 (new TestHost(1)) ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Errored and Ended should have been called")

[<Fact>]
let ``Exception in handler - Starting and Ended are raised even if Errored throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunData>(), new CallSpy<Exception>(Throws), new CallSpy<RunData>()
    let re = createRE2 (new TestHost(1)) ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Started and Ended should have been called")

[<Fact>]
let ``Exception in handler - Starting and Errored are raised even if Ended throws``() = 
    let ss = createSteps 2
    let (sh, erh, eh) = new CallSpy<RunData>(), new CallSpy<Exception>(), new CallSpy<RunData>(Throws)
    let re = createRE2 (new TestHost(1)) ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "Only Started and Errored should have been called")

[<Fact>]
let ``Cancellation - Executor raises all 3 events and stops execution``() = 
    let ss = createSteps 3
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 (new TestHost(1)) ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True
        ((areRdsSimillar sh.CalledWith rd) && (areRdsSimillar eh.CalledWith rd), 
         "Start and end events should be called with rundata")
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(err <> None && err = erh.CalledWith, "Error returned should also have been passed to error handler")

[<Fact>]
let ``Step throws - Executor raises all 3 events and stops execution``() = 
    let ss = Array.append [| new StepFunc(true) |] (createSteps 2)
    let (sh, erh, eh) = createHandlers()
    let re = createRE2 host ss I (sh, erh, eh)
    let rd, err = startRE re
    Assert.True(sh.Called && erh.Called && eh.Called, "All handlers should have been called")
    Assert.True(ss.[0].Called && not ss.[1].Called && not ss.[2].Called, "Only step 1 should have been executed")
    Assert.True(err <> None && err = erh.CalledWith, "Error returned should also have been passed to error handler")
