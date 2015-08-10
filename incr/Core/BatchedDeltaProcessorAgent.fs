module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.DeltaBatchingAgent

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

let rec private processor bulkSize timeout (bce : Event<_>) remainingTime messages 
        (mbox : Agent<_>) = 
    async { 
        let start = DateTime.Now
        let! msg = mbox.TryReceive(timeout = max 0 remainingTime)
        let elapsed = int (DateTime.Now - start).TotalMilliseconds
        match msg with
        | Some(msg) when List.length messages = bulkSize - 1 -> 
            bce.SafeTrigger(msg :: messages
                        |> List.rev
                        |> Array.ofList)
            return! processor bulkSize timeout bce timeout [] mbox
        | Some(msg) -> return! processor bulkSize timeout bce (remainingTime - elapsed) (msg :: messages) mbox
        | None when List.length messages <> 0 -> 
            bce.SafeTrigger(messages
                        |> List.rev
                        |> Array.ofList)
            return! processor bulkSize timeout bce timeout [] mbox
        | None -> return! processor bulkSize timeout bce timeout [] mbox
    }

let create bulkSize timeout bce = (processor bulkSize timeout bce timeout []) |> AutoCancelAgent.Start
