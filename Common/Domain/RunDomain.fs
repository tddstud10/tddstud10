namespace R4nd0mApps.TddStud10.Common.Domain

open System

type RunData = 
    { startTime : DateTime
      solutionPath : FilePath
      solutionSnapshotPath : FilePath
      solutionBuildRoot : FilePath
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
