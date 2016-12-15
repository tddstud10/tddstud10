module R4nd0mApps.TddStud10.Engine.Core.RunStateTrackerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.TestFramework
open System
open R4nd0mApps.TddStud10.Common.Domain

let ex = new InvalidOperationException("A mock method threw")

let createSM() = 
    let sm = new RunStateTracker()
    let cs = new CallSpy<RunState>()
    sm.RunStateChanged.Add(cs.Func >> ignore)
    sm, cs

let createSP() =
    RunStartParams.Create (EngineConfig()) DateTime.Now ~~"c:\\a\\b.sln"

let createRSS s =
    { status = s
      addendum = FreeFormatData ""
      runData = NoData }

let runTest2 (_ : RunStateTracker) (_ : CallSpy<RunState>) ts = 
    let sm, cs = createSM()
    let rd = createSP()

    let runOneTest () (e, exs) = 
        match e with
        | RunStarting -> sm.OnRunStarting(rd)
        | RunStepStarting(k) -> 
            sm.OnRunStepStarting({ sp = rd
                                   info = { name = RunStepName ""
                                            kind = k
                                            subKind = DiscoverTests } } )
        | RunStepError(k, s) -> 
            sm.OnRunStepError({ sp = rd
                                info = { name = RunStepName ""
                                         kind = k
                                         subKind = InstrumentBinaries }
                                rsr = { status = s
                                        addendum = FreeFormatData ""
                                        runData = NoData } })
        | RunStepEnded(k, s) -> 
            sm.OnRunStepEnd({ sp = rd
                              info = { name = RunStepName ""
                                       kind = k
                                       subKind = BuildSnapshot }
                              rsr = { status = s
                                      addendum = FreeFormatData ""
                                      runData = NoData } })
        | RunError(e) -> sm.OnRunError(e)
        Assert.Equal(cs.CalledWith, Some exs)
    ts |> List.fold runOneTest ()

let runTest ts = 
    let sm, cs = createSM()
    ts |> runTest2 sm cs

[<Fact>]
let ``Check initial state``() = 
    let sm, _ = createSM()
    Assert.Equal(Initial, sm.State)

[<Fact>]
let ``Handler crashes - Subsequent events are still fired``() = 
    let sm = new RunStateTracker()
    let cs = new CallSpy<RunState>(Throws(ex))
    sm.RunStateChanged.Add(cs.Func >> ignore)
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning ]
    |> runTest

[<Fact>]
let ``Happy Path - 1 - 2 builds and a test all passing``() = 
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Build), BuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Test), TestRunning
      RunStepEnded(Test, Succeeded), TestPassed ]
    |> runTest

[<Fact>]
let ``Happy Path - 2 - First build fails``() = 
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepError(Build, Failed), BuildFailureDetected
      RunStepEnded(Build, Failed), BuildFailed ]
    |> runTest

[<Fact>]
let ``Happy Path - 3 - Second build fails``() = 
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Build), BuildRunning
      RunStepError(Build, Failed), BuildFailureDetected
      RunStepEnded(Build, Failed), BuildFailed ]
    |> runTest

[<Fact>]
let ``Happy Path - 4 - Test fails``() = 
    let rss = createRSS Failed

    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Test), TestRunning
      RunStepError(Test, Failed), TestFailureDetected
      RunStepEnded(Test, Failed), TestFailed
      RunError(RunStepFailedException rss), TestFailed ]
    |> runTest

[<Fact>]
let ``Non-Happy Path - 0 - Run start recovers from engine error state``() = 
    [ // Event, Expected state
      RunStarting, Initial
      RunError(new InvalidOperationException()), EngineError
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning ]
    |> runTest

[<Fact>]
let ``Non-Happy Path - 1 - First build crashes``() =
    let rss = createRSS Aborted
    [ // Event, Expected state

      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepError(Build, Aborted), EngineErrorDetected
      RunStepEnded(Build, Aborted), EngineError
      RunError(RunStepFailedException rss), EngineError ]
    |> runTest

[<Fact>]
let ``Non-Happy Path - 2 - Second build crashes``() =
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Build), BuildRunning
      RunStepError(Build, Aborted), EngineErrorDetected
      RunStepEnded(Build, Aborted), EngineError ]
    |> runTest

[<Fact>]
let ``Non-Happy Path - 3 - Test crashes``() =
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Test), TestRunning
      RunStepError(Test, Aborted), EngineErrorDetected
      RunStepEnded(Test, Aborted), EngineError ]
    |> runTest

[<Fact>]
let ``Non-Happy Path - 4 - Run crashes after successful build and test``() =
    [ // Event, Expected state
      RunStarting, Initial
      RunStepStarting(Build), FirstBuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Build), BuildRunning
      RunStepEnded(Build, Succeeded), BuildPassed
      RunStepStarting(Test), TestRunning
      RunStepEnded(Test, Succeeded), TestPassed
      RunError(new InvalidOperationException()), EngineError ]
    |> runTest
