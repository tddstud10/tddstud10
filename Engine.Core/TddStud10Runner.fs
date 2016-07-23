namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Common.Domain

(* NOTE: 
   This should just be a wire up class. No business logic at all. 
   Hence unit tests not required. *)
type public TddStud10Runner private (re, agent, rst) = 
    
    static member public CreateRunStep(info : RunStepInfo, 
                                       func : Func<IRunExecutorHost, RunStartParams, RunStepInfo, RunStepResult>) : RunStep = 
        { info = info
          func = fun h sp i _ -> func.Invoke(h, sp, i) }
    
    static member public Create host runSteps = 
        let all f = 
            f
            |> RunStepFuncBehaviors.eventsPublisher
            |> RunStepFuncBehaviors.stepLogger
            |> RunStepFuncBehaviors.stepTimer
        
        let re = RunExecutor.Create host runSteps all
        let agent = new StaleMessageIgnoringAgent<EngineConfig * DateTime * FilePath>(re.Start >> ignore)
        let rst = new RunStateTracker()
        re.RunStarting.Add(rst.OnRunStarting)
        re.RunStepStarting.Add(rst.OnRunStepStarting)
        re.OnRunStepError.Add(rst.OnRunStepError)
        re.RunStepEnded.Add(rst.OnRunStepEnd)
        re.OnRunError.Add(rst.OnRunError)
        re.RunEnded.Add(rst.OnRunEnd)
        new TddStud10Runner(re, agent, rst)
    
    member public __.AttachHandlers (rsc : Handler<RunState>) (sh : Handler<RunStartParams>) (ssh : Handler<RunStepStartingEventArg>) 
           (serh : Handler<RunStepErrorEventArg>) (seh : Handler<RunStepEndedEventArg>) (erh : Handler<Exception>) 
           (eh : Handler<RunStartParams>) = 
        rst.RunStateChanged.AddHandler(rsc)
        re.RunStarting.AddHandler(sh)
        re.RunStepStarting.AddHandler(ssh)
        re.OnRunStepError.AddHandler(serh)
        re.RunStepEnded.AddHandler(seh)
        re.OnRunError.AddHandler(erh)
        re.RunEnded.AddHandler(eh)

    member public __.DetachHandlers (eh : Handler<RunStartParams>) (erh : Handler<Exception>) (seh : Handler<RunStepEndedEventArg>) 
           (serh : Handler<RunStepErrorEventArg>) (ssh : Handler<RunStepStartingEventArg>) (sh : Handler<RunStartParams>) 
           (rsc : Handler<RunState>) = 
        re.RunEnded.RemoveHandler(eh)
        re.OnRunError.RemoveHandler(erh)
        re.RunStepEnded.RemoveHandler(seh)
        re.OnRunStepError.RemoveHandler(serh)
        re.RunStepStarting.RemoveHandler(ssh)
        re.RunStarting.RemoveHandler(sh)
        rst.RunStateChanged.RemoveHandler(rsc)

    member public __.StartAsync cfg startTime slnPath token = agent.SendMessageAsync (cfg, startTime, slnPath) token
    member public __.StopAsync(token) = agent.StopAsync(token)
