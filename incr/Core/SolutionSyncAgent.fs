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
    
    let startOpOnSln<'TRes, 'TNEArgs> fDone fv (pose : Event<_>) (pone : Event<_ * Map<_, 'TRes> * 'TNEArgs>) fnep 
        (fe : Event<_>) sln (dg : DependencyGraph) (rmap : Map<ProjectId, 'TRes>) = 
        let roots = 
            dg.Roots()
            |> Seq.filter (rmap.ContainsKey >> not)
            |> Seq.sortBy dg.OutDegree
        if ((roots |> Seq.isEmpty) |> not)
        then 
            let fldr (rmap : Map<_, _>) pid = 
                pose.SafeTrigger(pid)
                let m = 
                    sln.DependencyMap.[pid]
                    |> Seq.map (fun id -> id, rmap.[id])
                    |> Map.ofSeq
                pone.SafeTrigger(sln, m, pid |> fnep)
                rmap |> Map.add pid (pid |> fv)
            roots |> Seq.fold fldr rmap
        else 
            fe.SafeTrigger(sln)
            fDone()
            rmap
    
    let completeOpOnProject<'TRes> (fe : Event<_>) (dg : AdjacencyGraph<_, _>) pid (rmap : Map<ProjectId, 'TRes>) r = 
        let rmap = rmap |> Map.updateWith (fun _ -> Some(r)) pid
        dg.RemoveVertex(pid) |> ignore
        fe.SafeTrigger(pid, r)
        rmap
    
    let loadSolution (mbox : Agent<_>) es sln (dg : DependencyGraph) (rmap : ProjectLoadResultMap) = 
        startOpOnSln (fun () -> mbox.Post(SyncAndBuildSolution)) (fun _ -> LoadInProgress) es.ProjectLoadStarting 
            es.ProjectLoadNeeded id es.LoadFinished sln dg rmap
    let syncAndBuildSolution es sln (pmap : ProjectMap) (dg : AdjacencyGraph<_, _>) (rmap : ProjectBuildResultMap) = 
        startOpOnSln id (fun p -> BuildInProgress pmap.[p]) es.ProjectSyncAndBuildStarting es.ProjectSyncAndBuildNeeded 
            (fun p -> pmap.[p]) es.LoadFinished sln dg rmap
    
    let continueOnInvalidState s msg = 
        Logger.logInfof "SSA: #### Continuing on invalid state transition. %A <- %A." s msg
        s
    
    let continueAfterUnloadingSolution (pmap : ProjectMap) = 
        pmap
        |> Map.values
        |> Seq.map (fun p -> p.Watchers)
        |> Seq.collect id
        |> Seq.iter (fun w -> w.Dispose())
        Unloaded
    
    let processUnloadedState la msg = 
        match msg with
        | LoadSolution sln -> 
            la.Es.LoadStarting.SafeTrigger(sln)
            let dg = sln.DGraph
            let plrm = Map.empty |> loadSolution la.MBox la.Es sln dg
            { Sln = sln
              PlrMap = plrm
              DGraph = dg }
            |> Loading
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(_, _) -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueOnInvalidState la.State msg
    
    let processLoadingState la msg (l : LoadingState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject(pid, plr) -> 
            let plrm = 
                plr
                |> completeOpOnProject la.Es.ProjectLoadFinished l.DGraph pid l.PlrMap
                |> loadSolution la.MBox la.Es l.Sln l.DGraph
            { l with PlrMap = plrm } |> Loading
        | SyncAndBuildSolution -> 
            la.Es.LoadStarting.SafeTrigger(l.Sln)
            let pmap, is = 
                l.PlrMap |> Seq.fold (fun (pmap, is) e -> 
                                match e.Value with
                                | LoadInProgress -> pmap, (sprintf "Project %A is still loading" e.Key) :: is
                                | LoadSuccess p -> pmap |> Map.add p.Id p, is
                                | LoadFailure is' -> pmap, is' @ is) (Map.empty, [])
            if (is
                |> List.isEmpty
                |> not)
            then 
                la.Es.LoadFailed.SafeTrigger(l.Sln)
                l |> Loading
            else 
                let dg = l.Sln.DGraph
                let bmap = Map.empty |> syncAndBuildSolution la.Es l.Sln pmap dg
                { Sln = l.Sln
                  PMap = pmap
                  PbrMap = bmap
                  DGraph = dg }
                |> SyncAndBuild
        | ProcessSyncedAndBuiltProject(_, _) -> continueOnInvalidState la.State msg
        | ProcessDeltas(_) -> continueOnInvalidState la.State msg
        | UnloadSolution -> continueOnInvalidState la.State msg
    
    let processSyncAndBuildState la msg (sab : SyncAndBuildState) = 
        match msg with
        | LoadSolution _ -> continueOnInvalidState la.State msg
        | ProcessLoadedProject _ -> continueOnInvalidState la.State msg
        | SyncAndBuildSolution -> continueOnInvalidState la.State msg
        | ProcessSyncedAndBuiltProject(p, pbr) -> 
            let bmap = 
                pbr
                |> completeOpOnProject la.Es.ProjectSyncAndBuildFinished sab.DGraph p.Id sab.PbrMap
                |> syncAndBuildSolution la.Es sab.Sln sab.PMap sab.DGraph
            { sab with PbrMap = bmap } |> SyncAndBuild
        | ProcessDeltas(_) -> 
            Logger.logWarnf "SSA: Not implemented yet. We should cancel current pipeline and retart."
            continueOnInvalidState la.State msg
        | UnloadSolution -> continueAfterUnloadingSolution sab.PMap
    
    let rec private loop la = 
        async { 
            Logger.logInfof "SSA: #### Starting loop in state = %A" la.State
            let! msg = la.MBox.Receive()
            Logger.logInfof "SSA: #### Processing message %A" msg
            match la.State with
            | Unloaded -> 
                let state' = processUnloadedState la msg
                return! loop { la with State = state' }
            | Loading l -> 
                let state' = processLoadingState la msg l
                return! loop { la with State = state' }
            | SyncAndBuild sab -> 
                let state' = processSyncAndBuildState la msg sab
                return! loop { la with State = state' }
        }
    
    let create es = 
        AutoCancelAgent.Start(fun mbox -> 
            loop { State = Unloaded
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
