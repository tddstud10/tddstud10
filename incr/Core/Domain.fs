namespace R4nd0mApps.TddStud10.Common.Domain

open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System.Diagnostics

[<DebuggerDisplay("{ToString()}")>]
type ProjectId = 
    | ProjectId of string
    override self.ToString() = 
        match self with
        | ProjectId s -> s

[<DebuggerDisplay("{Id}")>]
type Project = 
    { Id : ProjectId
      Path : FilePath
      Items : seq<FilePath>
      FileReferences : seq<FilePath>
      ProjectReferences : seq<ProjectId> }

type ProjectDependencyGraph = ArrayAdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>

type Solution = 
    { Name : string
      DependencyGraph : ProjectDependencyGraph
      Projects : Map<ProjectId, Project> }
