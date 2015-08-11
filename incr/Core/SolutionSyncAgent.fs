namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

module SolutionSyncAgent = 
    type LoopArgs = 
        { State : SolutionSyncState
          Es : SolutionSyncEvents
          MBox : Agent<SolutionSyncMessages> }
    
    let startOpOnSln<'TRes, 'TNEArgs when 'TRes : equality> fDone (pose : Event<_>) 
        (pone : Event<_ * Map<_, 'TRes option> * 'TNEArgs>) fnep (fe : Event<_>) sln (dg : DependencyGraph) 
        (rmap : Map<ProjectId, 'TRes option>) = 
        let roots = 
            dg.Roots()
            |> Seq.filter (rmap.ContainsKey >> not)
            |> Seq.sortBy dg.OutDegree
        
        let pending = rmap |> Seq.filter (fun kv -> kv.Value = None)
        if (not (roots |> Seq.isEmpty)) then 
            Logger.logInfof "SSA: %d roots available now. Starting operations on all of them." (roots |> Seq.length)
            let fldr (rmap : Map<_, _>) pid = 
                pose.SafeTrigger(pid)
                let m = 
                    sln.DependencyMap.[pid]
                    |> Seq.map (fun id -> id, rmap.[id])
                    |> Map.ofSeq
                pone.SafeTrigger(sln, m, pid |> fnep)
                rmap |> Map.add pid None
            false, roots |> Seq.fold fldr rmap
        else if (not (pending |> Seq.isEmpty)) then 
            Logger.logInfof "SSA: %d Project Ops still pending. Not seeking next set of roots." (pending |> Seq.length)
            false, rmap
        else 
            Logger.logInfof "SSA: All project operations complete. Done with solution."
            fe.SafeTrigger(sln)
            true, rmap
    
    let completeOpOnProject<'TRes> (pofe : Event<_>) (dg : AdjacencyGraph<_, _>) pid 
        (rmap : Map<ProjectId, 'TRes option>) r = 
        r |> Option.fold (fun _ r -> 
                 let rmap = rmap |> Map.updateWith (fun _ -> Some(Some(r))) pid
                 dg.RemoveVertex(pid) |> ignore
                 pofe.SafeTrigger(pid, r)
                 dg, rmap) (dg, Map.empty)
    
    let continueOnInvalidState s msg = 
        Logger.logInfof "SSA: #### Continuing on invalid state transition. %A <- %A." s msg
        s
    
    let continueAfterUnloadingSolution (pmap : ProjectMap) = 
        pmap
        |> Map.values
        |> Seq.map (fun p -> p.Watchers)
        |> Seq.collect id
        |> Seq.iter (fun w -> w.Dispose())
        ReadyToLoaded
    
    let handleMessagesInReadyToLoadState la msg = 
        match msg with
        | LoadSolution sln -> 
            la.Es.LoadStarting.SafeTrigger(sln)
            let dg = sln.DGraph
            let slnDone, plrm = 
                Map.empty
                |> startOpOnSln id la.Es.ProjectLoadStarting la.Es.ProjectLoadNeeded id la.Es.LoadFinished sln dg
            
            let l = 
                { Sln = sln
                  PlrMap = plrm
                  DGraph = dg }
            if (slnDone) then 
                la.MBox.Post(PrepareForSyncAndBuild)
                l |> FinishedLoad
            else l |> DoingLoad
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(_, _) -> continueOnInvalidState la.State msg
        | PrepareForSyncAndBuild -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueOnInvalidState la.State msg
    
    let handleMessagesInDoingLoadState la msg (l : LoadingState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject(pid, plr) -> 
            let dg = l.DGraph
            
            let slnDone, plrm = 
                plr
                |> Some
                |> completeOpOnProject la.Es.ProjectLoadFinished dg pid l.PlrMap
                ||> startOpOnSln id la.Es.ProjectLoadStarting la.Es.ProjectLoadNeeded id la.Es.LoadFinished l.Sln
            
            let l = { l with PlrMap = plrm }
            if (slnDone) then 
                la.MBox.Post(PrepareForSyncAndBuild)
                l |> FinishedLoad
            else l |> DoingLoad
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(_, _) -> continueOnInvalidState la.State msg
        | PrepareForSyncAndBuild -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueOnInvalidState la.State msg
    
    let handleMessagesInFinishedLoadState la msg (l : LoadingState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject(_, _) -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(_, _) -> continueOnInvalidState la.State msg
        | PrepareForSyncAndBuild ->
            let pmap = 
                l.PlrMap |> Seq.fold (fun pmap e -> 
                                match e.Value with
                                | Some r -> 
                                    match r with
                                    | LoadSuccess p -> pmap |> Map.add p.Id p
                                    | LoadFailure _ -> pmap
                                | None -> pmap) Map.empty
            if (pmap.Count <> l.PlrMap.Count) then 
                Logger.logInfof "SSA: Some projects (%d) have not loaded. Going back to FinishedLoad state. Will stay there till a full reset" (l.PlrMap.Count - pmap.Count) 
                la.Es.LoadFailed.SafeTrigger(l.Sln)
                l |> FinishedLoad
            else 
                la.MBox.Post(SyncAndBuildSolution)
                { ReadyToSyncAndBuildState.Sln = l.Sln
                  PMap = pmap } |> ReadyToSyncAndBuild
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueOnInvalidState la.State msg
    
    let handleMessagesInReadyToSyncAndBuildState la msg (sab : ReadyToSyncAndBuildState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution ->
            la.Es.SyncAndBuildStarting.SafeTrigger(sab.Sln)
            let dg = sab.Sln.DGraph
            let slnDone, plrm = 
                Map.empty
                |> startOpOnSln id la.Es.ProjectSyncAndBuildStarting la.Es.ProjectSyncAndBuildNeeded (fun p -> sab.PMap.[p]) la.Es.SyncAndBuildFinished sab.Sln dg
            
            let sab = 
                { Sln = sab.Sln
                  PMap = sab.PMap
                  PbrMap = plrm
                  DGraph = dg }
            if (slnDone) then 
                la.MBox.Post(PrepareForSyncAndBuild)
                sab |> FinishedSyncAndBuild
            else 
                sab |> DoingSyncAndBuild
        | ProcessSyncedAndBuiltProject(p, pbr) -> continueOnInvalidState la.State msg
        | PrepareForSyncAndBuild -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueAfterUnloadingSolution sab.PMap
    
    let handleMessagesInDoingSyncAndBuildState la msg (sab : SyncAndBuildState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(p, pbr) -> 
            let slnDone, bmap = 
                pbr
                |> Some
                |> completeOpOnProject la.Es.ProjectSyncAndBuildFinished sab.DGraph p.Id sab.PbrMap
                ||> startOpOnSln id la.Es.ProjectSyncAndBuildStarting la.Es.ProjectSyncAndBuildNeeded 
                        (fun p -> sab.PMap.[p]) la.Es.SyncAndBuildFinished sab.Sln
            let sab = { sab with PbrMap = bmap }
            if (slnDone) then 
                la.MBox.Post(PrepareForSyncAndBuild)
                sab |> FinishedSyncAndBuild
            else 
                sab |> DoingSyncAndBuild
        | PrepareForSyncAndBuild -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueAfterUnloadingSolution sab.PMap

    let handleMessagesInFinishedSyncAndBuildState la msg (sab : SyncAndBuildState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(p, pbr) -> continueOnInvalidState la.State msg
        | PrepareForSyncAndBuild ->
            let bmap = 
                sab.PbrMap |> Seq.fold (fun bmap e -> 
                                match e.Value with
                                | Some r -> 
                                    match r with
                                    | BuildSuccess(p, _) -> bmap |> Map.add e.Key ()
                                    | BuildFailure _ -> bmap
                                | None -> bmap) Map.empty
            if (bmap.Count <> sab.PMap.Count) then 
                la.Es.SyncAndBuildFailed.SafeTrigger(sab.Sln)
            { ReadyToSyncAndBuildState.Sln = sab.Sln
              PMap = sab.PMap } |> ReadyToSyncAndBuild
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueAfterUnloadingSolution sab.PMap
    
    let rec private loop la = 
        async { 
            Logger.logInfof "SSA: #### Starting loop in state = %A" la.State
            let! msg = la.MBox.Receive()
            Logger.logInfof "SSA: #### Processing message %A" msg
            match la.State with
            | ReadyToLoaded -> 
                let state' = handleMessagesInReadyToLoadState la msg
                return! loop { la with State = state' }
            | DoingLoad l -> 
                let state' = handleMessagesInDoingLoadState la msg l
                return! loop { la with State = state' }
            | FinishedLoad l -> 
                let state' = handleMessagesInFinishedLoadState la msg l
                return! loop { la with State = state' }
            | ReadyToSyncAndBuild rsab -> 
                let state' = handleMessagesInReadyToSyncAndBuildState la msg rsab
                return! loop { la with State = state' }
            | DoingSyncAndBuild sab -> 
                let state' = handleMessagesInDoingSyncAndBuildState la msg sab
                return! loop { la with State = state' }
            | FinishedSyncAndBuild sab -> 
                let state' = handleMessagesInFinishedSyncAndBuildState la msg sab
                return! loop { la with State = state' }
        }
    
    let create es = 
        AutoCancelAgent.Start(fun mbox -> 
            loop { State = ReadyToLoaded
                   Es = es
                   MBox = mbox })
    
    let load (agent : AutoCancelAgent<_>) (sln : DTESolution) = 
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
