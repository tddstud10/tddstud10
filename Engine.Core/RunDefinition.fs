namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.TestHost
open R4nd0mApps.TddStud10.Engine
open System

type FilePath = 
    | FilePath of string

type RunData = 
    { startTime : DateTime
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath
      sequencePoints : SequencePoints option
      discoveredUnitTests : DiscoveredUnitTests option
      codeCoverageResults : CoverageSession option
      executedTests : TestResults option }

type RunStepName = 
    | RunStepName of string

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool

type RunStepFunc = 
    IRunExecutorHost -> RunStepName -> (Event<RunStepName> * Event<RunStepName>) 
        -> RunData -> RunData

type RunStep = 
    { name : RunStepName
      func : RunStepFunc }

type RunSteps = RunStep array
