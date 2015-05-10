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
        new TddStud10Runner(re, agent, rst)
    
    member public t.AttachHandlers (rsc : Action<RunState>) (sh : Action<RunData>) (ssh : Action<RunStepEventArg>) 
           (serh : Action<RunStepEndEventArg>) (seh : Action<RunStepEndEventArg>) (erh : Action<Exception>) 
           (eh : Action<RunData>) = 
        t.re.RunStarting.AddHandler(fun s ea -> rst.OnRunStarting(ea))
        t.re.RunStepStarting.AddHandler(fun s ea -> rst.OnRunStepStarting(ea))
        t.re.OnRunStepError.AddHandler(fun s ea -> rst.OnRunStepError(ea))
        t.re.RunStepEnded.AddHandler(fun s ea -> rst.OnRunStepEnd(ea))
        t.re.OnRunError.AddHandler(fun s ea -> rst.OnRunError(ea))
        t.re.RunEnded.AddHandler(fun s ea -> rst.OnRunEnd(ea))
        rst.RunStateChanged.AddHandler(fun s ea -> rsc.Invoke(ea))
        t.re.RunStarting.AddHandler(fun s ea -> sh.Invoke(ea))
        t.re.RunStepStarting.AddHandler(fun s ea -> ssh.Invoke(ea))
        t.re.OnRunStepError.AddHandler(fun s ea -> serh.Invoke(ea))
        t.re.RunStepEnded.AddHandler(fun s ea -> seh.Invoke(ea))
        t.re.OnRunError.AddHandler(fun s ea -> erh.Invoke(ea))
        t.re.RunEnded.AddHandler(fun s ea -> eh.Invoke(ea))
    
    member public t.DetachHandlers (eh : Action<RunData>) (erh : Action<Exception>) (seh : Action<RunStepEndEventArg>) 
           (serh : Action<RunStepEndEventArg>) (ssh : Action<RunStepEventArg>) (sh : Action<RunData>) 
           (rsc : Action<RunState>) = 
        t.re.RunEnded.RemoveHandler(fun s ea -> eh.Invoke(ea))
        t.re.OnRunError.RemoveHandler(fun s ea -> erh.Invoke(ea))
        t.re.RunStepEnded.RemoveHandler(fun s ea -> seh.Invoke(ea))
        t.re.OnRunStepError.RemoveHandler(fun s ea -> serh.Invoke(ea))
        t.re.RunStepStarting.RemoveHandler(fun s ea -> ssh.Invoke(ea))
        t.re.RunStarting.RemoveHandler(fun s ea -> sh.Invoke(ea))
        rst.RunStateChanged.RemoveHandler(fun s ea -> rsc.Invoke(ea))
        t.re.RunEnded.RemoveHandler(fun s ea -> rst.OnRunEnd(ea))
        t.re.OnRunError.RemoveHandler(fun s ea -> rst.OnRunError(ea))
        t.re.RunStepEnded.RemoveHandler(fun s ea -> rst.OnRunStepEnd(ea))
        t.re.OnRunStepError.RemoveHandler(fun s ea -> rst.OnRunStepError(ea))
        t.re.RunStepStarting.RemoveHandler(fun s ea -> rst.OnRunStepStarting(ea))
        t.re.RunStarting.RemoveHandler(fun s ea -> rst.OnRunStarting(ea))
    
    member public t.StartAsync startTime slnPath token = t.agent.SendMessageAsync (startTime, FilePath slnPath) token
    member public t.StopAsync(token) = t.agent.StopAsync(token)
