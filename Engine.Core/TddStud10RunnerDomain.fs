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

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool

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

type RunStepFunc = IRunExecutorHost -> RunStepName -> RunStepKind -> RunStepEvents -> RunData -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      kind : RunStepKind
      func : RunStepFunc }

type RunSteps = RunStep array
