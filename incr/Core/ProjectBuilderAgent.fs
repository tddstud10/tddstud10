module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ProjectBuilderAgent

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

let rec private processor (pbce : Event<_>) (pbse : Event<_>) (mbox : Agent<_>) = 
    async { 
        Logger.logInfof "PBA: #### Starting loop."
        let! msg = mbox.Receive()
        Logger.logInfof "PBA: #### Processing message %A." msg
        match msg with
        | BuildProject psn -> 
            Logger.logInfof "PBA: Building project %A." psn
            let p, pbr = 
                match psn with
                | SnapshotSuccess(_, p, is) -> 
                    try 
                        let bos = Seq.empty
                        let ws = is
                        let r = BuildSuccess(p, bos, ws)
                        pbse.SafeTrigger(p.Id, bos, ws)
                        p, r
                    with e -> p, BuildFailure(p, is |> Seq.append [ e.ToString() ])
                | SnapshotFailure(p, is) -> p, BuildFailure(p, is)
            pbce.SafeTrigger(p, pbr)
            Logger.logInfof "PBA: Building %s done: Result = %A." p.Id.UniqueName pbr
        return! processor pbce pbse mbox
    }

let create pbce pbse = 
    (pbce, pbse)
    ||> processor
    |> AutoCancelAgent.Start
