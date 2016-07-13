namespace R4nd0mApps.TddStud10.Common.Domain

open System

type RunData = 
    | NoData
    | TestCases of PerDocumentLocationDTestCases
    | SequencePoints of PerDocumentSequencePoints
    | TestRunOutput of PerTestIdDResults * PerDocumentLocationTestFailureInfo * PerSequencePointIdTestRunId

type RunStartParams = 
    { startTime : DateTime
      testHostPath : FilePath
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath }

type RunStepInfo = 
    { name : RunStepName
      kind : RunStepKind
      subKind : RunStepSubKind }

type RunStepResult = 
    { status : RunStepStatus
      runData : RunData
      addendum : RunStepStatusAddendum }

exception RunStepFailedException of RunStepResult

type RunStepStartingEventArg = 
    { sp : RunStartParams
      info : RunStepInfo }

type RunStepErrorEventArg = 
    { sp : RunStartParams
      info : RunStepInfo
      rsr : RunStepResult }

type RunStepEndedEventArg = 
    { sp : RunStartParams
      info : RunStepInfo
      rsr : RunStepResult }

type RunStepEvents = 
    { onStart : Event<RunStepStartingEventArg>
      onError : Event<RunStepErrorEventArg>
      onFinish : Event<RunStepEndedEventArg> }

type RunStepFunc = IRunExecutorHost -> RunStartParams -> RunStepInfo -> RunStepEvents -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { info : RunStepInfo
      func : RunStepFunc }

type RunSteps = RunStep array
