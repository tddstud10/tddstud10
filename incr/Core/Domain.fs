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
type ProjectSnapshot = 
    | SnapshotSuccess of FilePath * Project * seq<string>
    | SnapshotFailure of Project * seq<string>
    member self.AsString = 
        match self with
        | SnapshotSuccess(sp, p, ws) -> sprintf "Success: %A. Ws : %d. Snapshot Path: %O." p.Id (ws |> Seq.length) sp
        | SnapshotFailure(p, es) -> sprintf "Failure: %A. Es: %d." p.Id (es |> Seq.length)

// ProjectLoader Agent Messages and Event Arguments
//
[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectLoadResult = 
    | LoadInProgress
    | LoadSuccess of Project
    | LoadFailure of list<string>
    member self.AsString = 
        match self with
        | LoadInProgress -> sprintf "InProgres"
        | LoadSuccess p -> sprintf "Success: %A." p
        | LoadFailure is -> sprintf "Failure: Issues = %d." (is |> Seq.length)

type ProjectLoadResultMap = Map<ProjectId, ProjectLoadResult>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectLoaderMessages = 
    | LoadProject of Solution * ProjectLoadResultMap * ProjectId
    member self.AsString = 
        match self with
        | LoadProject(_, _, pid) -> sprintf "LoadProject: %A." pid

// ProjectBuilder Agent Messages and Event Arguments
//
[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectBuildResult = 
    | BuildInProgress of Project
    | BuildSuccess of Project * seq<FilePath> * seq<string>
    | BuildFailure of Project * seq<string>
    member self.AsString = 
        match self with
        | BuildInProgress(p) -> sprintf "InProgress: %A" p.Id
        | BuildSuccess(p, bos, ws) -> 
            sprintf "Success: %A. Os : %d. Ws: %d." p.Id (bos |> Seq.length) (ws |> Seq.length)
        | BuildFailure(p, es) -> sprintf "Failure: %A. Es: %d." p.Id (es |> Seq.length)

type ProjectBuildResultMap = Map<ProjectId, ProjectBuildResult>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectBuilderMessages = 
    | BuildProject of ProjectSnapshot
    member self.AsString = 
        match self with
        | BuildProject psn -> sprintf "BuildProject: %A." psn

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

// SolutionSync Agent States, Messages and Event Arguments
//
[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type SolutionSyncMessages = 
    | LoadSolution of Solution
    | ProcessLoadedProject of ProjectId * ProjectLoadResult
    | SyncAndBuildSolution
    | ProcessSyncedAndBuiltProject of Project * ProjectBuildResult
    | ProcessDeltas of seq<FilePath * Project>
    | UnloadSolution
    member self.AsString = 
        match self with
        | LoadSolution s -> sprintf "LoadSolution: %O" s.Path
        | ProcessLoadedProject(pid, _) -> sprintf "ProcessLoadedProject: %s" pid.UniqueName
        | SyncAndBuildSolution -> sprintf "SyncAndBuildSolution"
        | ProcessSyncedAndBuiltProject(p, _) -> sprintf "ProcessSyncedAndBuiltProject %s" p.Id.UniqueName
        | ProcessDeltas ds -> sprintf "ProcessDeltas %d" (ds |> Seq.length)
        | UnloadSolution -> sprintf "UnloadSolution"

type SolutionSyncEvents = 
    { LoadStarting : Event<Solution>
      ProjectLoadStarting : Event<ProjectId>
      ProjectLoadNeeded : Event<Solution * ProjectLoadResultMap * ProjectId>
      ProjectLoadFinished : Event<ProjectId * ProjectLoadResult>
      ProjectSyncAndBuildStarting : Event<ProjectId>
      ProjectSyncAndBuildNeeded : Event<Solution * ProjectBuildResultMap * Project>
      ProjectSyncAndBuildFinished : Event<ProjectId * ProjectBuildResult>
      LoadFailed : Event<Solution>
      LoadFinished : Event<Solution> }

type DependencyGraph = AdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>

type LoadingState = 
    { Sln : Solution
      PlrMap : ProjectLoadResultMap
      DGraph : DependencyGraph }

type SyncAndBuildState = 
    { Sln : Solution
      PMap : ProjectMap
      PbrMap : ProjectBuildResultMap
      DGraph : DependencyGraph }

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type SolutionSyncState = 
    | Unloaded
    | Loading of LoadingState
    | SyncAndBuild of SyncAndBuildState
    member self.AsString = 
        match self with
        | Unloaded -> sprintf "Unloaded"
        | Loading l -> sprintf "Loading S = %A. PlrMap = %d items." l.Sln l.PlrMap.Count
        | SyncAndBuild sab -> sprintf "Loading S = %A. PlrMap = %d items." sab.Sln sab.PbrMap.Count

// DeltaBatching Agent Messages and Event Arguments
//
type DeltaBatchingMessages = 
    | ProcessDelta of FilePath * Project
