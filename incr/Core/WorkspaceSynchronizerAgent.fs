namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

type WorkspaceSynchronizerAgent(projectLoader) = 
    let mutable disposed = false
    let onLoading = Event<_>()
    let onProjectLoading = Event<_>()
    let onProjectLoaded = Event<_>()
    let onLoaded = Event<_>()
    
    let rec processor (sln : Solution option) (rmap : ProjectLoadResultTrackingMap) (dg : AdjacencyGraph<_, _>) 
            (mbox : Agent<_>) = 
        let startProjectLoad sln (rmap : ProjectLoadResultTrackingMap) p = 
            Common.safeExec (fun () -> onProjectLoading.Trigger(p))
            sln.DependencyMap.[p]
            |> Seq.map (fun p -> p, rmap.[p])
            |> Seq.map 
                   (fun (p, res) -> 
                   p, 
                   res 
                   |> Option.fold (fun _ r -> r) 
                          (ProjectLoadResult.createFailedResult 
                               [ sprintf "Project %A not loaded for unknown reasons." p ]))
            |> Map.ofSeq
            |> projectLoader sln p
            rmap.Add(p, None)
        
        let completeProjectLoad (dg : AdjacencyGraph<_, _>) p (rmap) res = 
            let rmap = rmap |> Map.updateWith (fun _ -> Some(Some(res))) p
            dg.RemoveVertex(p) |> ignore
            Common.safeExec (fun () -> onProjectLoaded.Trigger(p, res))
            rmap
        
        let loadSolution sln (dg : AdjacencyGraph<_, _>) (rmap : ProjectLoadResultTrackingMap) = 
            let roots = 
                dg.Roots()
                |> Seq.filter (rmap.ContainsKey >> not)
                |> Seq.sortBy dg.OutDegree
            
            let rmap = roots |> Seq.fold (startProjectLoad sln) rmap
            if (roots |> Seq.length) = 0 then Common.safeExec (fun () -> onLoaded.Trigger(sln))
            rmap
        
        async { 
            Logger.logInfof "Entering agent processor. Sln = %A" sln
            let! msg = mbox.Receive()
            match msg with
            | LoadSolution sln -> 
                // TODO: Existing RMAP should be empty - put in a check here.
                // TODO: Dg must also be empty
                Logger.logInfof "Agent processor - Load Solution. Solution = %A" sln
                Common.safeExec (fun () -> onLoading.Trigger(sln))
                let dg = 
                    (sln.DependencyMap |> Map.keys).ToAdjacencyGraph<_, _> 
                        (Func<_, _>(fun t -> sln.DependencyMap.[t] |> Seq.map (fun s -> SEquatableEdge<_>(s, t))))
                let rmap = rmap |> loadSolution sln dg
                return! processor (Some sln) rmap dg mbox
            | ProcessLoadedProject(p, res) -> 
                match sln with
                | Some sln -> 
                    Logger.logInfof "Agent processor - ProjectLoaded. Solution = %A. Project = %A. Result = %A" sln p 
                        res
                    let rmap = 
                        res
                        |> completeProjectLoad dg p rmap
                        |> loadSolution sln dg
                    return! processor (Some sln) rmap dg mbox
                | None -> 
                    Logger.logInfof 
                        "Agent processor - ProjectLoaded - Ignoring message. Solution = %A. Project = %A. Result = %A" 
                        sln p res
                    return! processor sln rmap dg mbox
            | UnloadSolution -> 
                match sln with
                | Some sln -> 
                    Logger.logInfof "Agent processor - Unload. Solution = %A." sln
                    rmap
                    |> Map.values
                    |> Seq.choose id
                    |> Seq.map (fun res -> res.ItemWatchers)
                    |> Seq.collect id
                    |> Seq.iter (fun iw -> (iw :> IDisposable).Dispose())
                | None -> Logger.logInfof "Agent processor - Unload - Ignoring message. Solution = %A." sln
                return! processor None Map.empty (AdjacencyGraph<_, _>()) mbox
            | ProcessDeltas ds -> 
                let loadInProgress = 
                    rmap
                    |> Map.values
                    |> Seq.exists (fun res -> res = None)
                if (loadInProgress) then 
                    Logger.logInfof "Agent processor - Message %A - Ignoring message as load is in progress. Solution = %A." msg sln
                    return! processor sln rmap dg mbox
                else 
                    Logger.logInfof "Agent processor - Message %A - Processing deltas. Solution = %A. Deltas: %A" msg 
                        sln ds
                    return! processor sln rmap dg mbox
        }
    
    let agent = AutoCancelAgent.Start(processor None Map.empty (AdjacencyGraph<_, _>()))
    
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
        |> LoadSolution
        |> agent.Post
    
    member __.ProjectLoaded pres = 
        pres
        |> ProcessLoadedProject
        |> agent.Post
    
    member __.ProcessDeltas ds = 
        ds
        |> ProcessDeltas
        |> agent.Post
    
    member __.Unload() = 
        Logger.logInfof "Starting unload."
        UnloadSolution |> agent.Post
