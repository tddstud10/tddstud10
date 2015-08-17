namespace R4nd0mApps.TddStud10.Common.Domain

open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

type Agent<'T> = MailboxProcessor<'T>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectId = 
    { UniqueName : string
      Id : Guid }
    member self.AsString = sprintf "%s (%O)" self.UniqueName self.Id

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type Project = 
    { Id : ProjectId
      Path : FilePath
      Items : seq<FilePath>
      FileReferences : seq<FilePath>
      ProjectReferences : seq<ProjectId>
      Watchers : seq<IDisposable> }
    member self.AsString = 
        sprintf "%O (Is = %d, FRs = %d, PRs = %d)" self.Path (self.Items |> Seq.length) 
            (self.FileReferences |> Seq.length) (self.ProjectReferences |> Seq.length)

type ProjectMap = Map<ProjectId, Project>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type Solution = 
    { Path : FilePath
      DependencyMap : Map<ProjectId, Set<ProjectId>>
      Solution : EnvDTE.Solution }
    member self.AsString = sprintf "%s (Dependencies: %d)" (self.Path.ToString()) self.DependencyMap.Count

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type OperationResult<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure
    member self.AsString = 
        match self with
        | Success _ -> sprintf "Success"
        | Failure _ -> sprintf "Failure"

type ProjectSnapshot = OperationResult<FilePath * seq<string>, seq<string>>

// ProjectLoader Agent Messages and Event Arguments
//
type ProjectLoadResult = OperationResult<Project, seq<string>>

type ProjectLoadResultMap = Map<ProjectId, ProjectLoadResult option>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectLoaderMessages = 
    | LoadProject of Solution * ProjectLoadResultMap * ProjectId
    member self.AsString = 
        match self with
        | LoadProject(_, _, pid) -> sprintf "LoadProject: %A." pid

[<CLIMutable>]
type ProjectLoadEvents =
    { LoadStarting : Event<ProjectId>
      LoadFinished : Event<ProjectId * ProjectLoadResult> }

// ProjectBuilder Agent Messages and Event Arguments
//
type ProjectBuildResult = OperationResult<seq<FilePath> * seq<string>, seq<string>>

type ProjectBuildResultMap = Map<ProjectId, ProjectBuildResult option>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectBuilderMessages = 
    | BuildProject of Project * ProjectSnapshot
    member self.AsString = 
        match self with
        | BuildProject (p, psn) -> sprintf "BuildProject: %A, %A." p psn

[<CLIMutable>]
type ProjectBuildEvents =
    { BuildStarting : Event<Project>
      BuildFinished : Event<Project * ProjectBuildResult> }

// ProjectSynchronizer Agent Messages and Event Arguments
//
type ProjectSyncResult = ProjectSnapshot

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectSyncMessages = 
    | SyncProject of Solution * ProjectBuildResultMap * Project
    member self.AsString = 
        match self with
        | SyncProject(_, _, p) -> sprintf "SyncProject: %A." p

[<CLIMutable>]
type ProjectSyncEvents =
    { SyncStarting : Event<Project>
      SyncFinished : Event<Project * ProjectSyncResult> }

// SolutionSync Agent States, Messages and Event Arguments
//
type DependencyGraph = AdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>

type LoadingState = 
    { Sln : Solution
      PlrMap : ProjectLoadResultMap
      DGraph : DependencyGraph }

type ReadyToSyncAndBuildState = 
    { Sln : Solution
      PMap : ProjectMap }

type SyncAndBuildState = 
    { Sln : Solution
      PMap : ProjectMap
      PbrMap : ProjectBuildResultMap
      DGraph : DependencyGraph }

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type SolutionSyncState = 
    | ReadyToLoaded
    | DoingLoad of LoadingState
    | FinishedLoad of LoadingState
    | ReadyToSyncAndBuild of ReadyToSyncAndBuildState
    | DoingSyncAndBuild of SyncAndBuildState
    | FinishedSyncAndBuild of SyncAndBuildState
    member self.AsString = 
        match self with
        | ReadyToLoaded -> sprintf "ReadyToLoaded"
        | DoingLoad l -> sprintf "DoingLoad S = %A. PlrMap = %d items." l.Sln l.PlrMap.Count
        | FinishedLoad l -> sprintf "FinishedLoad S = %A. PlrMap = %d items." l.Sln l.PlrMap.Count
        | ReadyToSyncAndBuild rsab -> sprintf "ReadyToSyncAndBuild S = %A. # of Projects = %d." rsab.Sln rsab.PMap.Count
        | DoingSyncAndBuild sab -> sprintf "DoingSyncAndBuild S = %A. PlrMap = %d items." sab.Sln sab.PbrMap.Count
        | FinishedSyncAndBuild sab -> sprintf "FinishedSyncAndBuild S = %A. PlrMap = %d items." sab.Sln sab.PbrMap.Count

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type SolutionSyncMessages = 
    | LoadSolution of Solution
    | ProcessLoadedProject of ProjectId * ProjectLoadResult
    | SyncAndBuildSolution
    | ProcessSyncedAndBuiltProject of Project * ProjectBuildResult
    | PrepareForSyncAndBuild
    | ProcessDeltas of seq<FilePath * Project>
    | UnloadSolution
    member self.AsString = 
        match self with
        | LoadSolution s -> sprintf "LoadSolution: %O" s.Path
        | ProcessLoadedProject(pid, _) -> sprintf "ProcessLoadedProject: %s" pid.UniqueName
        | SyncAndBuildSolution -> sprintf "SyncAndBuildSolution"
        | ProcessSyncedAndBuiltProject(p, _) -> sprintf "ProcessSyncedAndBuiltProject %s" p.Id.UniqueName
        | PrepareForSyncAndBuild -> sprintf "PrepareForSyncAndBuild"
        | ProcessDeltas ds -> sprintf "ProcessDeltas %d" (ds |> Seq.length)
        | UnloadSolution -> sprintf "UnloadSolution"

type SolutionOperationResult = OperationResult<unit, unit>

type SolutionSyncEvents = 
    { LoadStarting : Event<Solution>
      ProjectLoadNeeded : Event<Solution * ProjectLoadResultMap * ProjectId>
      LoadFinished : Event<Solution * SolutionOperationResult> 
      SyncAndBuildStarting : Event<Solution>
      ProjectSyncAndBuildNeeded : Event<Solution * ProjectBuildResultMap * Project>
      SyncAndBuildFinished : Event<Solution * SolutionOperationResult> }

// DeltaBatching Agent Messages and Event Arguments
//
type DeltaBatchingMessages = 
    | ProcessDelta of FilePath * Project

[<CLIMutable>]
type IncrementalBuildPipelineEvents =
    { LoadStarting : Solution -> unit 
      ProjectLoadEvents : ProjectLoadEvents
      LoadFinished : Solution * SolutionOperationResult -> unit
      SyncAndBuildStarting : Solution -> unit
      ProjectSyncEvents : ProjectSyncEvents
      ProjectBuildEvents : ProjectBuildEvents
      SyncAndBuildFinished : Solution * SolutionOperationResult -> unit }

(*
Simplifications:
- GOAL: View in UI
---------
v Event fired only by respective agents
v Combine Sln Finished/Failed Events
- Parameter to IBP is a struct
---------
---------
- ASAP Cancellation of all pipeline operations on next event
---------
- ProjectLoaded is not a seperate agent - not required, revisit when making the engine external hosted
 *)
