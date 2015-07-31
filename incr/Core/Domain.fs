namespace R4nd0mApps.TddStud10.Common.Domain

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Generic
open System.Diagnostics

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
      ProjectReferences : seq<ProjectId> }
    member self.AsString = 
        sprintf "%O (Is = %d, FRs = %d, PRs = %d)" self.Path (self.Items |> Seq.length) 
            (self.FileReferences |> Seq.length) (self.ProjectReferences |> Seq.length)

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectSnapshot = 
    { Path : FilePath
      Issues : exn list }
    member self.AsString = sprintf "%O (Is = %d)" self.Path (self.Issues |> Seq.length)

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type ProjectLoadResult = 
    { Status : bool
      Warnings : seq<string>
      Errors : seq<string>
      Outputs : seq<string> }
    member self.AsString = 
        sprintf "%b (Ws = %d, Es = %d, Os = %d)" self.Status (self.Warnings |> Seq.length) 
            (self.Errors |> Seq.length) (self.Outputs |> Seq.length)

type ProjectLoadResultTrackingMap = Map<ProjectId, ProjectLoadResult option>
type ProjectLoadResultMap = Map<ProjectId, ProjectLoadResult>

[<DebuggerDisplay("{AsString}")>]
[<StructuredFormatDisplay("{AsString}")>]
type Solution = 
    { Path : FilePath
      DependencyMap : Map<ProjectId, Set<ProjectId>>
      Solution : EnvDTE.Solution }
    member self.AsString = sprintf "%s (Dependencies: %d)" (self.Path.ToString()) self.DependencyMap.Count

type WorkspaceMessages = 
    | Load of Solution
    | ProjectLoaded of ProjectId * ProjectLoadResult
    | ProcessDeltas
    | Unload

type ProjectLoaderMessages = 
    | LoadProject of Solution * ProjectId * ProjectLoadResultMap

type ProjectSnapshoterMessages = 
    | SnapshotProject of ProjectId
