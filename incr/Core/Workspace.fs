namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System

(* DELETE THIS ONCE WE MERGE BACK *)
module Common = 
    let safeExec (f : unit -> unit) = 
        try 
            f()
        with _ -> ()

(*Logger.logErrorf "Exception thrown: %s." (ex.ToString())*)
(* DELETE THIS ONCE WE MERGE BACK  *)

[<AutoOpen>]
module ProjectExtensions = 
    open System
    
    type Project with
        static member fromDTEProject (p : DTEProject) : Project = 
            { Id = 
                  { UniqueName = p.UniqueName
                    Id = p.ProjectGuid }
              Path = p.FullName |> FilePath
              Items = 
                  p.GetProjectItems()
                  |> Seq.filter DTEProjectItem.isSupported
                  |> Seq.map DTEProjectItem.getFiles
                  |> Seq.collect id
              FileReferences = p.GetFileReferences()
              ProjectReferences = p.GetProjectReferences() }

type Workspace() = 
    let loadComplete = new Event<_>()
    member public __.LoadComplete = loadComplete.Publish
    member __.Load(sln : DTESolution) : unit = 
        let projects = sln.GetProjects() |> Seq.map Project.fromDTEProject
        
        let dependencies = 
            sln.GetBuildDependencies()
            |> Seq.map (fun bd -> 
                   { UniqueName = bd.Project.UniqueName
                     Id = bd.Project.ProjectGuid }, 
                   bd.RequiredProjects :?> obj [] |> Seq.map (fun o -> 
                                                         let p = o :?> DTEProject
                                                         { UniqueName = p.UniqueName
                                                           Id = p.ProjectGuid }))
            |> Map.ofSeq
        
        let s = 
            { Path = sln.FullName |> FilePath
              Projects = 
                  projects
                  |> Seq.map (fun p -> p.Id, p)
                  |> Map.ofSeq
              DependencyMap = dependencies }
        
        Common.safeExec (fun () -> loadComplete.Trigger(s))
