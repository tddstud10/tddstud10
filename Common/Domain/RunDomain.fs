namespace R4nd0mApps.TddStud10.Common.Domain

open System

type RunData =
    | NoData
    | TestCases of PerAssemblyTestCases
    | SequencePoints of PerDocumentSequencePoints
    | TestRunOutput of PerTestIdResults * PerAssemblySequencePointsCoverage 

type RunStartParams = 
    { startTime : DateTime
      testHostPath : FilePath
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath }

type RunStepResult = 
    { startParams : RunStartParams
      name : RunStepName
      kind : RunStepKind
      subKind : RunStepSubKind
      status : RunStepStatus
      runData : RunData
      addendum : RunStepStatusAddendum }

exception RunStepFailedException of RunStepResult

type RunStepStartingEventArg = 
    { startParams : RunStartParams
      name : RunStepName
      subKind : RunStepSubKind
      kind : RunStepKind }

type RunStepErrorEventArg = 
    { rsr : RunStepResult }

type RunStepEndedEventArg =
    { rsr : RunStepResult }

type RunStepEvents = 
    { onStart : Event<RunStepStartingEventArg>
      onError : Event<RunStepErrorEventArg>
      onFinish : Event<RunStepEndedEventArg> }

type RunStepFunc = IRunExecutorHost -> RunStartParams -> RunStepName -> RunStepKind -> RunStepSubKind -> RunStepEvents -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      kind : RunStepKind
      subKind : RunStepSubKind
      func : RunStepFunc }

type RunSteps = RunStep array
