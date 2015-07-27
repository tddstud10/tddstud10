namespace R4nd0mApps.TddStud10.Common.Domain

open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Generic
open System.Diagnostics

[<DebuggerDisplay("{UniqueName} - {Id}")>]
type ProjectId = 
    { UniqueName : string
      Id : Guid }

[<DebuggerDisplay("Id = {Id}")>]
type Project = 
    { Id : ProjectId
      Path : FilePath
      Items : seq<FilePath>
      FileReferences : seq<FilePath>
      ProjectReferences : seq<ProjectId> }

[<DebuggerDisplay("Path = {Path}")>]
type ProjectSnapshot = 
    { Path : FilePath
      Issues : exn list }

[<DebuggerDisplay("Status = {Status}")>]
type ProjectBuildResult = 
    { Status : bool
      Warnings : seq<string>
      Errors : seq<string>
      Outputs : seq<string> }

[<DebuggerDisplay("Path = {Path}")>]
type Solution = 
    { Path : FilePath
      DependencyMap : IDictionary<ProjectId, seq<ProjectId>>
      Projects : Map<ProjectId, Project> }
