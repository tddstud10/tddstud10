namespace R4nd0mApps.TddStud10.Engine.Core

open System.Threading
open System.Threading.Tasks

[<Measure>]
type ms

type Duration = 
    | Duration of int<ms>

type AgentMessage<'T> = 
    | Data of 'T * AsyncReplyChannel<'T option * 'T list>
    | Pause of Duration
    | Stop of AsyncReplyChannel<unit>

// TODO: REFACTOR: Pull in Rx and replace this with Debounce.
type StaleMessageIgnoringAgent<'T>(f) = 
    let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger

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
                                    logger.logInfof "SMIAgent: We have been asked to process message: %O." m
                                    if c > 1 then 
                                        let more = c - 1
                                        logger.logInfof "SMIAgent: Flushing stale messages. Ignored %O. (%d remaining)." 
                                            m more
                                        return! messageLoop more p (m :: il)
                                    else 
                                        logger.logInfof "SMIAgent: Flushed all stale messages. Processing %O." m
                                        f m
                                        rc.Reply(Some m, il)
                                        return! messageLoop inbox.CurrentQueueLength (Some m) il
                                | Pause(Duration t) -> 
                                    logger.logInfof "SMIAgent: We have been asked to pause for %A ms" t
                                    Thread.Sleep(t / 1<ms>)
                                    return! messageLoop inbox.CurrentQueueLength None []
                                | Stop(rc) -> 
                                    logger.logInfof "SMIAgent: We have been asked to stop"
                                    rc.Reply()
                                    return ()
                            | None -> 
                                logger.logInfof "SMIAgent: TryReceive timed out. Should not really have happened"
                                return! messageLoop inbox.CurrentQueueLength None []
                        with exn -> 
                            logger.logErrorf 
                                "SMIAgent: Exception while processing. Reporting, ignoring and continuing: Exception %O" 
                                exn
                            errorEvent.Trigger(exn)
                            return! messageLoop inbox.CurrentQueueLength None []
                    }
                messageLoop inbox.CurrentQueueLength None [])
        // NOTE: Is a better idea to restart the agent?
        agent.Error.Add
            (fun e -> logger.logErrorf "SMIAgent: There was an unhandled error. This is not expected: Exception %O" e)
        agent
    
    let agent = createAgent f
    member __.OnError = errorEvent.Publish
    member __.SendMessage(m : 'T) = agent.PostAndReply(fun rc -> Data(m, rc))
    member __.SendMessageAsync (m : 'T) token = 
        Async.StartAsTask(agent.PostAndAsyncReply(fun rc -> Data(m, rc)), TaskCreationOptions.None, token)
    member __.PauseAsync(t : int<ms>) = agent.Post(Pause(Duration t))
    member __.Stop() = agent.PostAndReply(fun rc -> Stop(rc))
    member __.StopAsync(token) = 
        Async.StartAsTask(agent.PostAndAsyncReply(fun rc -> Stop(rc)), TaskCreationOptions.None, token)
