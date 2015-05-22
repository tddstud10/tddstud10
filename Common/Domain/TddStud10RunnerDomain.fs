namespace R4nd0mApps.TddStud10.Common.Domain

open System
open System.Collections
open Microsoft.VisualStudio.TestPlatform.ObjectModel

[<CustomEquality; CustomComparison>]
type FilePath = 
    | FilePath of string
    
    override x.ToString() = 
        match x with
        | FilePath s -> s
    
    override x.GetHashCode() = 
        match x with
        | FilePath s -> s.ToUpperInvariant().GetHashCode()
    
    override x.Equals(yObj) = 
        match yObj with
        | :? FilePath as y -> 
            let FilePath x, FilePath y = x, y
            x.ToUpperInvariant().Equals(y.ToUpperInvariant(), StringComparison.Ordinal)
        | _ -> false
    
    interface IComparable<FilePath> with
        member x.CompareTo(y : FilePath) : int = 
            let FilePath x, FilePath y = x, y
            x.ToUpperInvariant().CompareTo(y.ToUpperInvariant())
    
    interface IComparable with
        member x.CompareTo(yObj : obj) : int = 
            match yObj with
            | :? FilePath as y -> (x :> IComparable<FilePath>).CompareTo(y)
            | _ -> 1
    
    interface IEquatable<FilePath> with
        member x.Equals(y : FilePath) : bool = x.Equals(y)

type AssemblyId = 
    | AssemblyId of Guid

type MdTokenRid = 
    | MdTokenRid of uint32

type SequencePointId = 
    | SequencePointId of int

type DocumentCoordinate =
    | DocumentCoordinate of int

type TestRunInstanceId =
    | TestRunInstanceId of int

[<CLIMutable>]
type TestId = 
    { source : FilePath
      document : FilePath
      line : DocumentCoordinate }

[<CLIMutable>]
type TestRunId = 
    { testId : TestId
      testRunInstanceId : TestRunInstanceId }

[<CLIMutable>]
type TestRunResult =
    { result : TestOutcome }

[<CLIMutable>]
type MethodId = 
    { assemblyId : AssemblyId
      mdTokenRid : MdTokenRid }


[<CLIMutable>]
type SequencePoint = 
    { methodId : MethodId
      id : SequencePointId
      document : FilePath
      startLine : DocumentCoordinate
      startColumn : DocumentCoordinate
      endLine : DocumentCoordinate
      endColumn : DocumentCoordinate }

[<CLIMutable>]
type SequencePointCoverage = 
    { methodId : MethodId
      sequencePointId : SequencePointId 
      testRunId : TestRunId }

// =================================================

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

// ==================================================

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

// ==========================================================

type public IRunExecutorHost = 
    abstract CanContinue : unit -> bool
    abstract RunStateChanged : RunState -> unit
