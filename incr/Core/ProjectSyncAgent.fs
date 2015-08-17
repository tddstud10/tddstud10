module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ProjectSyncAgent

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

let private syncProject s rmap p = 
    Success("" |> FilePath, Seq.empty)

let rec private processor (pses : ProjectSyncEvents) dce (mbox : Agent<_>) = 
    async { 
        Logger.logInfof "PSA: #### Starting loop."
        let! msg = mbox.Receive()
        Logger.logInfof "PSA: #### Processing message %A." msg
        match msg with
        | SyncProject(s, rmap, p) -> 
            Logger.logInfof "PSA: Syncing project %A." p
            pses.SyncStarting.SafeTrigger(p)
            let psn = 
                try 
                    syncProject s rmap p
                with e -> e.ToString() |> Seq.singleton |> Failure
            do! Async.Sleep 3000
            pses.SyncFinished.SafeTrigger(p, psn)
            Logger.logInfof "PSA: Done syncing project %s: Result = %A." p.Id.UniqueName psn
        return! processor pses dce mbox
    }

let create dce pses = 
    (pses, dce)
    ||> processor
    |> AutoCancelAgent.Start
//            let res = 
//                p
//                |> ProjectExtensions.createSnapshot s
//                |> ProjectExtensions.fixupProject rmap
//                |> ProjectExtensions.buildSnapshot
//            let res = 
//                (p, res, dce) 
//                |||> ProjectExtensions.subscribeToChangeNotifications
