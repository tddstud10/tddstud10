namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

(*
Cleanup:
- How to make multiple agents listen to same queue
- Add descend by max outgoing edges
- Handling Agent
- Refactor: Graphextensions:Map to dependency graph
- Refactor: Enumerate roots in desc order

v Tweak data model
v Dont pass entire rmap between agents
  v Fix Logger.logErrorf "XXXXXX"
- Split project loader
 *)

type Workspace(projectLoader) = 
    let mutable disposed = false
    let onLoading = new Event<_>()
    let onProjectLoading = new Event<_>()
    let onProjectLoaded = new Event<_>()
    let onLoaded = new Event<_>()
    
    let rec processor (sln : Solution option) (rmap : ProjectLoadResultTrackingMap) (dg : AdjacencyGraph<_, _>) 
            (inbox : MailboxProcessor<_>) = 
        let startProjectLoad sln (rmap : ProjectLoadResultTrackingMap) p = 
            Common.safeExec (fun () -> onProjectLoading.Trigger(p))
            sln.DependencyMap.[p]
            |> Seq.map (fun p -> p, rmap.[p])
            |> Seq.map 
                    (fun (p, res) -> 
                    p, 
                    res 
                    |> Option.fold (fun _ r -> r) 
                            (ProjectLoadResult.createFailedResult [ sprintf "Project %A not loaded for unknown reasons." p ]))
            |> Map.ofSeq
            |> projectLoader sln p
            rmap.Add(p, None)
        
        let completeProjectLoad (rmap : ProjectLoadResultTrackingMap) (dg : AdjacencyGraph<_, _>) p res = 
            let rmap = rmap |> Map.updateWith (fun _ -> Some(Some(res))) p
            dg.RemoveVertex(p) |> ignore
            Common.safeExec (fun () -> onProjectLoaded.Trigger(p, res))
            rmap
        
        let loadSolution sln (rmap : ProjectLoadResultTrackingMap) (dg : AdjacencyGraph<_, _>) = 
            let roots = 
                dg.Roots()
                |> Seq.filter (rmap.ContainsKey >> not)
                |> Seq.sortBy dg.OutDegree
            
            let rmap = roots |> Seq.fold (startProjectLoad sln) rmap
            if (roots |> Seq.length) = 0 then Common.safeExec (fun () -> onLoaded.Trigger(sln))
            rmap
        
        async { 
            Logger.logInfof "Entering agent processor. Sln = %A" sln
            let! msg = inbox.Receive()
            match msg with
            | Load sln -> 
                // TODO: Existing RMAP should be empty - put in a check here.
                // TODO: Dg must also be empty
                Logger.logInfof "Agent processor - Load Solution. Solution = %A" sln
                Common.safeExec (fun () -> onLoading.Trigger(sln))
                let dg = 
                    (sln.DependencyMap |> Map.keys).ToAdjacencyGraph<_, _> 
                        (Func<_, _>(fun t -> sln.DependencyMap.[t] |> Seq.map (fun s -> SEquatableEdge<_>(s, t))))
                let rmap = (sln, rmap, dg) |||> loadSolution
                return! processor (Some sln) rmap dg inbox
            | ProjectLoaded(p, res) -> 
                match sln with
                | Some sln -> 
                    Logger.logInfof "Agent processor - Project Loaded. Solution = %A. Project = %A. Result = %A" sln p 
                        res
                    let rmap = completeProjectLoad rmap dg p res
                    let rmap = (sln, rmap, dg) |||> loadSolution
                    return! processor (Some sln) rmap dg inbox
                | None -> 
                    Logger.logInfof 
                        "Agent processor - Project Loaded - Ignoring message. Solution = %A. Project = %A. Result = %A" 
                        sln p res
                    return! processor sln rmap dg inbox
            | msg -> 
                // Do ?nothing if load operation is in progress
                Logger.logInfof "Agent processor - Message %A - Ignoring message. Solution = %A." msg sln
                return! processor sln rmap dg inbox
        }
    
    let agent = AutoCancelAgent.Start(processor None (Map.empty) (AdjacencyGraph<_, _>()))
    
    let dispose disposing = 
        if not disposed then 
            if (disposing) then (agent :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    member __.OnLoading = onLoading.Publish
    member __.OnProjectLoading = onProjectLoading.Publish
    member __.OnProjectLoaded = onProjectLoaded.Publish
    member __.OnLoaded = onLoaded.Publish
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.Load(sln : DTESolution) = 
        Logger.logInfof "Starting load of %s" sln.FullName
        let deps = 
            sln.GetBuildDependencies()
            |> Seq.map (fun bd -> 
                   let deps = 
                       bd.RequiredProjects :?> obj []
                       |> Seq.map (fun o -> 
                              let p = o :?> DTEProject
                              { UniqueName = p.UniqueName
                                Id = p.ProjectGuid })
                       |> Set.ofSeq
                   { UniqueName = bd.Project.UniqueName
                     Id = bd.Project.ProjectGuid }, deps)
            |> Map.ofSeq
        { Path = sln.FullName |> FilePath
          DependencyMap = deps
          Solution = sln }
        |> Load
        |> agent.Post
    
    member __.ProjectLoaded pres = 
        pres
        |> ProjectLoaded
        |> agent.Post
