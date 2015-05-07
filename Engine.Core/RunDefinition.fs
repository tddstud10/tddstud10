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
      buildConsoleOutput : string option
      codeCoverageResults : CoverageSession option
      executedTests : TestResults option
      testConoleOutput : string option }

type RunStepName = 
    | RunStepName of string

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool

type RunStepEventArgType =
    RunStepName * RunData

type RunStepEvent =
    Event<RunStepName * RunData>

type RunStepEventPair =
    RunStepEvent * RunStepEvent

type RunStepFunc = IRunExecutorHost -> RunStepName -> (RunStepEvent * RunStepEvent) -> RunData -> RunData

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      func : RunStepFunc }

type RunSteps = RunStep array
