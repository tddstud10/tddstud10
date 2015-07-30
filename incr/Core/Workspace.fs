namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

type WorkspaceMessages = 
    | Load of Solution
    | ProjectLoaded of ProjectId * ProjectLoadResult
    | ProcessDeltas
    | Unload

(*
Cleanup:
- Map to dependency graph
- Add descend by max outgoing edges
- Handling Agent
- Changed dictionary to map
- Dont pass entire rmap between agents
- How to make multiple agents listen to same queue
- Split project loader
- Fix Logger.logErrorf "XXXXXX"
 *)

type Workspace(projLoader) = 
    let mutable disposed = false
    let onLoading = new Event<_>()
    let onProjectLoading = new Event<_>()
    let onProjectLoaded = new Event<_>()
    let onLoaded = new Event<_>()
    
    let rec processor (sln : Solution option) (rmap : ProjectLoadResultMap) (dg : AdjacencyGraph<_, _>) 
            (inbox : MailboxProcessor<_>) = 
        let startProjectLoad sln (rmap : ProjectLoadResultMap) p = 
            Common.safeExec (fun () -> onProjectLoading.Trigger(p))
            (sln, p, rmap) |||> projLoader
            rmap.Add(p, None)
        
        let completeProjectLoad (rmap : ProjectLoadResultMap) (dg : AdjacencyGraph<_, _>) p res = 
            rmap.[p] <- Some(res)
            dg.RemoveVertex(p) |> ignore
            Common.safeExec (fun () -> onProjectLoaded.Trigger(p, res))
        
        let loadSolution sln (rmap : ProjectLoadResultMap) (dg : AdjacencyGraph<_, _>) = 
            let sinks = AlgorithmExtensions.Sinks<_, _>(dg) |> Seq.filter (rmap.ContainsKey >> not)
            sinks |> Seq.iter (startProjectLoad sln rmap)
            if (sinks |> Seq.length) = 0 then Common.safeExec (fun () -> onLoaded.Trigger(sln))
        
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
                    (sln.DependencyMap :> IDictionary<_, _>).Keys.ToAdjacencyGraph<_, _> 
                        (Func<_, _>(fun s -> sln.DependencyMap.[s] |> Seq.map (fun t -> SEquatableEdge<_>(s, t))))
                (sln, rmap, dg) |||> loadSolution
                return! processor (Some sln) rmap dg inbox
            | ProjectLoaded(p, res) -> 
                match sln with
                | Some sln -> 
                    Logger.logInfof "Agent processor - Project Loaded. Solution = %A. Project = %A. Result = %A" sln p 
                        res
                    completeProjectLoad rmap dg p res
                    (sln, rmap, dg) |||> loadSolution
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
    
    let agent = AutoCancelAgent.Start(processor None (ProjectLoadResultMap()) (AdjacencyGraph<_, _>()))
    
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
