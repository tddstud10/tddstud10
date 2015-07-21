namespace Core

open FSharpVSPowerTools
open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections
open VSLangProj

type ProjectId = 
    | ProjectId of string

type Project = 
    { Id : ProjectId
      Path : FilePath
      Items : seq<FilePath>
      FileReferences : seq<FilePath>
      ProjectReferences : seq<ProjectId> }

type ProjectDependencyGraph = ArrayAdjacencyGraph<ProjectId, SEquatableEdge<ProjectId>>

type Workspace = 
    { dependencyGraph : ProjectDependencyGraph
      projects : Map<ProjectId, Project> }

// ========================================
module List = 
    // TODO: Unique Name - do attempt etc.
    let ofEnumerator<'T> (e : IEnumerator) = 
        [ while e.MoveNext() do
              yield e.Current :?> 'T ]

[<AutoOpen>]
module EnvDTEExtensions = 
    let private unsupportedProjectKinds = 
        [ EnvDTE.Constants.vsProjectKindMisc.ToUpperInvariant()
          EnvDTE.Constants.vsProjectKindSolutionItems.ToUpperInvariant()
          EnvDTE.Constants.vsProjectKindUnmodeled.ToUpperInvariant() ]
        |> Set.ofSeq
    
    type EnvDTE.Project with
        
        // TODO: Unique Name - do attempt etc.
        member self.IsSupported = 
            self.Kind.ToUpper()
            |> unsupportedProjectKinds.Contains
            |> not
        
        // TODO: Unique Name - do attempt etc.
        member self.Items = 
            let rec loop (l : list<EnvDTE.ProjectItem>) (is : list<EnvDTE.ProjectItem>) : list<EnvDTE.ProjectItem> = 
                match is with
                | [] -> l
                | pi :: pis -> 
                    let l = [ pi ] @ l @ (pi.ProjectItems.GetEnumerator() |> List.ofEnumerator<EnvDTE.ProjectItem>)
                    loop l pis
            self.ProjectItems.GetEnumerator()
            |> List.ofEnumerator<EnvDTE.ProjectItem>
            |> loop []
        
        // TODO: DRY Violation
        member self.ProjectReferences = 
            (self.Object :?> VSProject).References
            |> Seq.cast<Reference>
            |> Seq.choose (fun ref -> maybe { let! ref = Option.ofNull ref
                                              let! p = Option.attempt (fun _ -> ref.SourceProject)
                                              return! Option.ofNull p })
            // TODO: Unique Name - do attempt etc.
            |> Seq.map (fun p -> p.UniqueName |> ProjectId)
        
        member self.FileReferences = 
            (self.Object :?> VSProject).References
            |> Seq.cast<Reference>
            |> Seq.choose (fun ref -> 
                   maybe { 
                       let! ref = Option.ofNull ref
                       let! isCopyLocal = Option.attempt (fun _ -> ref.CopyLocal)
                       let! p = Option.attempt (fun _ -> ref.SourceProject)
                       let! path = Option.attempt (fun _ -> ref.Path)
                       return! if isCopyLocal && p = null then Option.ofNull path
                               else None
                   })
            |> Seq.map (fun p -> p |> FilePath)
    
    let private unsupportedProjectItemKinds = 
        [ EnvDTE.Constants.vsProjectItemKindPhysicalFolder.ToUpperInvariant() ] |> Set.ofSeq
    
    type EnvDTE.ProjectItem with
        
        // TODO: Unique Name - do attempt etc.
        member self.IsSupported = 
            self.Kind.ToUpper()
            |> unsupportedProjectItemKinds.Contains
            |> not
        
        // TODO: Unique Name - do attempt etc.
        member self.Files = 
            seq { 
                for i in 1..(int) self.FileCount do
                    yield self.FileNames((int16) i) |> FilePath
            }

module WorkspaceLoader = 
    // TODO: Unique Name - do attempt etc.
    let load (sln : EnvDTE.Solution) : Workspace = 
        let projects = 
            sln.Projects.GetEnumerator()
            |> List.ofEnumerator<EnvDTE.Project>
            |> Seq.filter (fun pi -> pi.IsSupported)
            |> Seq.map (fun p -> 
                   let id = p.UniqueName |> ProjectId
                   id, 
                   { Id = id
                     Path = p.FullName |> FilePath
                     Items = 
                         p.Items
                         |> Seq.filter (fun pi -> pi.IsSupported)
                         |> Seq.map (fun pi -> pi.Files)
                         |> Seq.collect Operators.id
                     FileReferences = p.FileReferences
                     ProjectReferences = p.ProjectReferences })
        
        let dependencies = 
            seq { 
                for i in 1..(int) sln.SolutionBuild.BuildDependencies.Count do
                    yield sln.SolutionBuild.BuildDependencies.Item(i)
            }
            |> Seq.map (fun bd -> 
                   let s = bd.Project.UniqueName |> ProjectId
                   bd.RequiredProjects :?> obj []
                   |> Array.map (fun o -> (o :?> EnvDTE.Project).UniqueName |> ProjectId)
                   |> Seq.map (fun t -> SEquatableEdge<ProjectId>(s, t)))
            |> Seq.collect id
        
        { projects = projects |> Map.ofSeq
          dependencyGraph = GraphExtensions.ToAdjacencyGraph<_, SEquatableEdge<_>>(dependencies).ToArrayAdjacencyGraph() }
