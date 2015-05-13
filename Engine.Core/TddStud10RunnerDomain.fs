namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.TestHost
open R4nd0mApps.TddStud10.Engine
open System

type FilePath = 
    | FilePath of string
    override t.ToString() = 
        match t with
        | FilePath s -> s

type RunData = 
    { startTime : DateTime
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath
      sequencePoints : SequencePoints option
      discoveredUnitTests : DiscoveredUnitTests option
      codeCoverageResults : CoverageSession option
      executedTests : TestResults option }

type RunStepKind = 
    | Build
    | Test
    override t.ToString() = 
        match t with
        | Build -> "Build"
        | Test -> "Test"

type RunStepName = 
    | RunStepName of string
    override t.ToString() = 
        match t with
        | RunStepName s -> s

type RunStepStatus = 
    | Aborted
    | Succeeded
    | Failed
    override t.ToString() = 
        match t with
        | Aborted -> "Aborted"
        | Succeeded -> "Succeeded"
        | Failed -> "Failed"

type RunStepStatusAddendum = 
    | FreeFormatData of string
    | ExceptionData of Exception
    override t.ToString() = 
        match t with
        | FreeFormatData s -> s
        | ExceptionData e -> e.ToString()

type RunStepResult = 
    { name : RunStepName
      kind : RunStepKind
      status : RunStepStatus
      addendum : RunStepStatusAddendum
      runData : RunData }

exception RunStepFailedException of RunStepResult

type RunStepEventArg = 
    { name : RunStepName
      kind : RunStepKind
      runData : RunData }

type RunStepEndEventArg = RunStepResult

type RunStepEvents = 
    { onStart : Event<RunStepEventArg>
      onError : Event<RunStepEndEventArg>
      onFinish : Event<RunStepEndEventArg> }

(*
    Combination of the following variables:
    - EngineOK, EngineError 
    - Unknown, Red, Green
    - None, Build, Test
    - Running, Idle

    Principles:
    - [Any State] -> [Initial state] : Only by run start run
    - Idle -> Runing : Any of the start steps
    - Runing -> Idle : Ay of the end steps

    Validity of combinations:
      [EngineError]
    - Unknown, None, Idle          [Final State]    [An internal engine error has occured]

      [EngineOK]
    - Unknown, None, Idle          [Initial state]  [The state right at the beginning]
    - Unknown, None, Running                        
    - Unknown, Build, Running
    - x Unknown, Build, Idle
    - x Unknown, Test, Running
    - x Unknown, Test, Idle
    - x Red, None, Running
    - x Red, None, Idle
    - Red, Build, Running
    - Red, Build, Idle             [Final State]
    - Red, Test, Running
    - Red, Test, Idle              [Final State]
    - x Green, None, Running
    - x Green, None, Idle
    - Green, Build, Running
    - Green, Build, Idle
    - Green, Test, Running
    - Green, Test, Idle            [Final State]
 *)
type RunState = 
    | Initial
    | EngineErrorDetected
    | EngineError
    | FirstBuildRunning
    | BuildFailureDetected
    | BuildFailed
    | TestFailureDetected
    | TestFailed
    | BuildRunning
    | BuildPassed
    | TestRunning
    | TestPassed

type RunEvent =
    | RunStarting
    | RunStepStarting of RunStepKind
    | RunStepError of RunStepKind * RunStepStatus
    | RunStepEnded of RunStepKind * RunStepStatus
    | RunError of Exception

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool
    abstract RunStateChanged : RunState -> unit

type RunStepFunc = IRunExecutorHost -> RunStepName -> RunStepKind -> RunStepEvents -> RunData -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      kind : RunStepKind
      func : RunStepFunc }

type RunSteps = RunStep array
