module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ProjectBuilderAgent

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

let rec private processor (pbes : ProjectBuildEvents) (mbox : Agent<_>) = 
    async { 
        Logger.logInfof "PBA: #### Starting loop."
        let! msg = mbox.Receive()
        Logger.logInfof "PBA: #### Processing message %A." msg
        match msg with
        | BuildProject(p, psn) -> 
            Logger.logInfof "PBA: Building project %A." psn
            pbes.BuildStarting.SafeTrigger(p)
            let p, pbr = 
                match psn with
                | Success(_, is) -> 
                    try 
                        let bos = Seq.empty
                        let ws = is
                        let r = Success(bos, ws)
                        p, r
                    with e -> p, Failure(is |> Seq.append [ e.ToString() ])
                | Failure is -> p, Failure(is)
            do! Async.Sleep 3000
            pbes.BuildFinished.SafeTrigger(p, pbr)
            Logger.logInfof "PBA: Done building %s: Result = %A." p.Id.UniqueName pbr
        return! processor pbes mbox
    }

let create pbes = 
    pbes
    |> processor
    |> AutoCancelAgent.Start
