namespace R4nd0mApps.TddStud10.Engine.Core

open System

(* NOTE: 
   This should just be a wire up class. No business logic at all. 
   Hence unit tests not required. *)
type public TddStud10Runner private (re, agent, rst) = 
    member private t.re = re
    member private t.agent = agent
    member private t.rst = rst
    
    static member public CreateRunStep(kind : RunStepKind, name : RunStepName, 
                                       func : Func<IRunExecutorHost, RunStepName, RunStepKind, RunData, RunStepResult>) : RunStep = 
        { kind = kind
          name = name
          func = fun h n k es rd -> func.Invoke(h, n, k, rd) }
    
    static member public Create host runSteps = 
        let all f = 
            f
            |> RunStepFuncBehaviors.eventsPublisher
            |> RunStepFuncBehaviors.stepLogger
            |> RunStepFuncBehaviors.stepTimer
        
        let re = RunExecutor.Create host runSteps all
        let agent = new StaleMessageIgnoringAgent<DateTime * FilePath>(re.Start >> ignore)
        let rst = new RunStateTracker()
        re.RunStarting.Add(rst.OnRunStarting)
        re.RunStepStarting.Add(rst.OnRunStepStarting)
        re.OnRunStepError.Add(rst.OnRunStepError)
        re.RunStepEnded.Add(rst.OnRunStepEnd)
        re.OnRunError.Add(rst.OnRunError)
        re.RunEnded.Add(rst.OnRunEnd)
        new TddStud10Runner(re, agent, rst)
    
    member public t.AttachHandlers (rsc : Handler<RunState>) (sh : Handler<RunData>) (ssh : Handler<RunStepEventArg>) 
           (serh : Handler<RunStepEndEventArg>) (seh : Handler<RunStepEndEventArg>) (erh : Handler<Exception>) 
           (eh : Handler<RunData>) = 
        rst.RunStateChanged.AddHandler(rsc)
        t.re.RunStarting.AddHandler(sh)
        t.re.RunStepStarting.AddHandler(ssh)
        t.re.OnRunStepError.AddHandler(serh)
        t.re.RunStepEnded.AddHandler(seh)
        t.re.OnRunError.AddHandler(erh)
        t.re.RunEnded.AddHandler(eh)

    member public t.DetachHandlers (eh : Handler<RunData>) (erh : Handler<Exception>) (seh : Handler<RunStepEndEventArg>) 
           (serh : Handler<RunStepEndEventArg>) (ssh : Handler<RunStepEventArg>) (sh : Handler<RunData>) 
           (rsc : Handler<RunState>) = 
        t.re.RunEnded.RemoveHandler(eh)
        t.re.OnRunError.RemoveHandler(erh)
        t.re.RunStepEnded.RemoveHandler(seh)
        t.re.OnRunStepError.RemoveHandler(serh)
        t.re.RunStepStarting.RemoveHandler(ssh)
        t.re.RunStarting.RemoveHandler(sh)
        rst.RunStateChanged.RemoveHandler(rsc)

    member public t.StartAsync startTime slnPath token = t.agent.SendMessageAsync (startTime, FilePath slnPath) token
    member public t.StopAsync(token) = t.agent.StopAsync(token)
