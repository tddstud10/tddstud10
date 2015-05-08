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

type RunStepName = 
    | RunStepName of string

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool

type RunStepStatus = 
    | Aborted
    | Succeeded
    | Failed

type RunStepStatusAddendum = 
    | FreeFormatData of string
    | ExceptionData of Exception

type RunStepResult = 
    { name : RunStepName
      kind : RunStepKind
      status : RunStepStatus
      addendum : RunStepStatusAddendum option
      runData : RunData }

type RunStepEventArg = RunStepName * RunData

type RunStepErrorEventArg = RunStepResult

type RunStepEvents = 
    { onStart : Event<RunStepEventArg>
      onError : Event<RunStepErrorEventArg>
      onFinish : Event<RunStepEventArg> }

type RunStepFunc = IRunExecutorHost -> RunStepName -> RunStepKind -> RunStepEvents -> RunData -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      kind : RunStepKind
      func : RunStepFunc }

type RunSteps = RunStep array
