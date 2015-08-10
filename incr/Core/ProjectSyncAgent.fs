module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ProjectSyncAgent

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

let private syncProject s rmap p = SnapshotSuccess("" |> FilePath, p, Seq.empty)

let rec private processor (psce : Event<_>) dce (mbox : Agent<_>) = 
    async { 
        Logger.logInfof "PSA: #### Starting loop."
        let! msg = mbox.Receive()
        Logger.logInfof "PSA: #### Processing message %A." msg
        match msg with
        | SyncProject(s, rmap, p) -> 
            Logger.logInfof "PSA: Syncing project %A." p
            let psn = 
                try 
                    syncProject s rmap p
                with e -> (p, e.ToString() |> Seq.singleton) |> SnapshotFailure
            psn |> psce.SafeTrigger
            Logger.logInfof "PSA: Syncing project %s done: Result = %A." p.Id.UniqueName psn
        return! processor psce dce mbox
    }

let create dce psce = 
    (psce, dce)
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
