namespace R4nd0mApps.TddStud10.Common.Domain

open System

type RunData1 =
    | TestCases of PerAssemblyTestCases
    | SequencePoints of PerDocumentSequencePoints
    | SequencePointsCoverage of PerAssemblySequencePointsCoverage 
    | TestResults of PerTestIdResults

type RunStartParams = 
    { startTime : DateTime
      testHostPath : FilePath
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath }

type RunData = 
    { startParams : RunStartParams
      testsPerAssembly : PerAssemblyTestCases option
      sequencePoints : PerDocumentSequencePoints option
      codeCoverageResults : PerAssemblySequencePointsCoverage option
      executedTests : PerTestIdResults option }

type RunStepResult = 
    { name : RunStepName
      kind : RunStepKind
      status : RunStepStatus
      addendum : RunStepStatusAddendum
      runData : RunData }

exception RunStepFailedException of RunStepResult

type RunStepStartingEventArg = 
    { name : RunStepName
      kind : RunStepKind
      runData : RunData }

type RunStepErrorEventArg = 
    { rsr : RunStepResult }

type RunStepEndedEventArg =
    { rsr : RunStepResult }

type RunStepEvents = 
    { onStart : Event<RunStepStartingEventArg>
      onError : Event<RunStepErrorEventArg>
      onFinish : Event<RunStepEndedEventArg> }

type RunStepFunc = IRunExecutorHost -> RunStepName -> RunStepKind -> RunStepEvents -> RunData -> RunStepResult

type RunStepFuncWrapper = RunStepFunc -> RunStepFunc

type RunStep = 
    { name : RunStepName
      kind : RunStepKind
      func : RunStepFunc }

type RunSteps = RunStep array
