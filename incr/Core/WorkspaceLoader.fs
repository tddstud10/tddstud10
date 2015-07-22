namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain

(* DELETE THIS ONCE WE MERGE BACK *)
module Common = 
    let safeExec (f : unit -> unit) = 
        try 
            f()
        with ex -> ()

(*Logger.logErrorf "Exception thrown: %s." (ex.ToString())*)
(* DELETE THIS ONCE WE MERGE BACK  *)

[<AutoOpen>]
module ProjectExtensions = 
    type Project with
        static member fromDTEProject (p : DTEProject) : Project = 
            { Id = p.UniqueName |> ProjectId
              Path = p.FullName |> FilePath
              Items = 
                  p.GetProjectItems()
                  |> Seq.filter DTEProjectItem.isSupported
                  |> Seq.map DTEProjectItem.getFiles
                  |> Seq.collect id
              FileReferences = p.GetFileReferences()
              ProjectReferences = p.GetProjectReferences() }

type WorkspaceLoader(sln : DTESolution) = 
    let loadComplete = new Event<_>()
    
    [<CLIEvent>]
    member public __.LoadComplete = loadComplete.Publish
    
    member self.Load() : unit = 
        let projects = sln.GetProjects() |> Seq.map Project.fromDTEProject
        
        let dependencies = 
            sln.GetBuildDependencies()
            |> Seq.map (fun bd -> 
                   let s = bd.Project.UniqueName |> ProjectId
                   bd.RequiredProjects :?> obj []
                   |> Array.map (fun o -> (o :?> DTEProject).UniqueName |> ProjectId)
                   |> Seq.map (fun t -> SEquatableEdge<ProjectId>(s, t)))
            |> Seq.collect id
        
        let dg = AdjacencyGraph<ProjectId, SEquatableEdge<_>>()
        projects
        |> Seq.map (fun p -> p.Id)
        |> dg.AddVertexRange
        |> ignore
        dependencies
        |> dg.AddEdgeRange
        |> ignore
        let w = 
            { Projects = 
                  projects
                  |> Seq.map (fun p -> p.Id, p)
                  |> Map.ofSeq
              DependencyGraph = dg.ToArrayAdjacencyGraph() }
        Common.safeExec (fun () -> loadComplete.Trigger(w))
