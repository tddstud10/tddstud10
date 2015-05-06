namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Engine.Diagnostics
open System.IO
open System.Threading

[<Measure>]
type ms

type Duration = 
    | Duration of int<ms>

type AgentMessage<'T> = 
    | Data of 'T * AsyncReplyChannel<'T option * 'T list>
    | Pause of Duration
    | Stop of AsyncReplyChannel<unit>

// NOTE: Do we need to implement IDisposable?
type StaleMessageIgnoringAgent<'T>(f) = 
    let errorEvent = Event<_>()
    
    let createAgent f = 
        let agent = 
            MailboxProcessor<AgentMessage<'T>>.Start(fun inbox -> 
                let rec messageLoop c p il = 
                    async { 
                        try 
                            let! msg = inbox.TryReceive()
                            match msg with
                            | Some m -> 
                                match m with
                                | Data(m, rc) -> 
                                    Logger.logInfof "SMIAgent: We have been asked to process message: %O." m
                                    if c > 1 then 
                                        let more = c - 1
                                        Logger.logInfof "SMIAgent: Flushing stale messages. Ignored %O. (%d remaining)." 
                                            m more
                                        return! messageLoop more p (m :: il)
                                    else 
                                        Logger.logInfof "SMIAgent: Flushed all stale messages. Processing %O." m
                                        f m
                                        rc.Reply(Some m, il)
                                        return! messageLoop inbox.CurrentQueueLength (Some m) il
                                | Pause(Duration t) -> 
                                    Logger.logInfof "SMIAgent: We have been asked to pause for %A ms" t
                                    Thread.Sleep(t / 1<ms>)
                                    return! messageLoop inbox.CurrentQueueLength None []
                                | Stop(rc) -> 
                                    Logger.logInfof "SMIAgent: We have been asked to stop"
                                    rc.Reply()
                                    return ()
                            | None -> 
                                Logger.logInfof "SMIAgent: TryReceive timed out. Should not really have happened"
                                return! messageLoop inbox.CurrentQueueLength None []
                        with exn -> 
                            Logger.logErrorf 
                                "SMIAgent: Exception while processing. Reporting, ignoring and continuing: Exception %O" 
                                exn
                            errorEvent.Trigger(exn)
                            return! messageLoop inbox.CurrentQueueLength None []
                    }
                messageLoop inbox.CurrentQueueLength None [])
        // NOTE: Is a better idea to restart the agent?
        agent.Error.Add
            (fun e -> Logger.logErrorf "SMIAgent: There was an unhandled error. This is not expected: Exception %O" e)
        agent
    
    let agent = createAgent f
    member this.OnError = errorEvent.Publish
    member this.SendMessage(m : 'T) = agent.PostAndReply(fun rc -> Data(m, rc))
    member this.SendMessageAsync(m : 'T) = agent.PostAndAsyncReply(fun rc -> Data(m, rc))
    member this.PauseAsync(t : int<ms>) = agent.Post(Pause(Duration t))
    member this.Stop() = agent.PostAndReply(fun rc -> Stop(rc))
    member this.StopAsync() = agent.PostAndAsyncReply(fun rc -> Stop(rc))
