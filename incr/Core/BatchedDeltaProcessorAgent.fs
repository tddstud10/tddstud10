namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Generic

type BatchedDeltaProcessorAgent(bulkSize, timeout) = 
    let mutable disposed = false
    let batchEvent = Event<_>()
    
    let agent : AutoCancelAgent<BatchedDeltaProcessorMessages> = 
        AutoCancelAgent.Start(fun agent -> 
            let rec loop remainingTime messages = 
                async { 
                    let start = DateTime.Now
                    let! msg = agent.TryReceive(timeout = max 0 remainingTime)
                    let elapsed = int (DateTime.Now - start).TotalMilliseconds
                    match msg with
                    | Some(msg) when List.length messages = bulkSize - 1 -> 
                        batchEvent.Trigger(msg :: messages
                                           |> List.rev
                                           |> Array.ofList)
                        return! loop timeout []
                    | Some(msg) -> return! loop (remainingTime - elapsed) (msg :: messages)
                    | None when List.length messages <> 0 -> 
                        batchEvent.Trigger(messages
                                           |> List.rev
                                           |> Array.ofList)
                        return! loop timeout []
                    | None -> return! loop timeout []
                }
            loop timeout [])

    let dispose disposing = 
        if not disposed then 
            if (disposing) then (agent :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.BatchProduced = batchEvent.Publish
    member __.Post(p) = agent.Post(p)

