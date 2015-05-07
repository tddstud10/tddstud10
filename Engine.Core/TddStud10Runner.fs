namespace R4nd0mApps.TddStud10.Engine.Core

open System

(* NOTE: 
   This should just be a wire up class. No business logic at all. 
   Hence unit tests not required. *)
type public TddStud10Runner private (re, agent) = 
    member private t.re = re
    member private t.agent = agent
    
    static member public CreateRunStep(name : string, func : Func<IRunExecutorHost, string, RunData, RunData>) : RunStep = 
        { name = RunStepName name
          func = fun h (RunStepName n) es rd -> func.Invoke(h, n, rd) }
    
    static member public Create host runSteps = 
        let all f = 
            f 
            |> RunStepFuncBehaviors.runStepFuncEventsPublisher 
            |> RunStepFuncBehaviors.runStepFuncLogger
            |> RunStepFuncBehaviors.runStepFuncTimer
        let re = RunExecutor.Create host runSteps all
        let agent = new StaleMessageIgnoringAgent<DateTime * FilePath>(re.Start >> ignore)
        new TddStud10Runner(re, agent)
    
    member public t.AttachHandlers (sh : Action<RunData>) (ssh : Action<RunStepEventArgType>) 
           (seh : Action<RunStepEventArgType>) (erh : Action<Exception>) (eh : Action<RunData>) = 
        t.re.RunStarting.AddHandler(fun s ea -> sh.Invoke(ea))
        t.re.RunStepStarting.AddHandler(fun s ea -> ssh.Invoke(ea))
        t.re.RunStepEnded.AddHandler(fun s ea -> seh.Invoke(ea))
        t.re.OnRunError.AddHandler(fun s ea -> erh.Invoke(ea))
        t.re.RunEnded.AddHandler(fun s ea -> eh.Invoke(ea))
    
    member public t.DetachHandlers (eh : Action<RunData>) (erh : Action<Exception>) (seh : Action<RunStepEventArgType>) 
           (ssh : Action<RunStepEventArgType>) (sh : Action<RunData>) = 
        t.re.RunEnded.RemoveHandler(fun s ea -> eh.Invoke(ea))
        t.re.OnRunError.RemoveHandler(fun s ea -> erh.Invoke(ea))
        t.re.RunStepEnded.RemoveHandler(fun s ea -> seh.Invoke(ea))
        t.re.RunStepStarting.RemoveHandler(fun s ea -> ssh.Invoke(ea))
        t.re.RunStarting.RemoveHandler(fun s ea -> sh.Invoke(ea))
    
    member public t.StartAsync startTime slnPath = t.agent.SendMessageAsync(startTime, FilePath slnPath)
    member public t.StopAsync() = t.agent.StopAsync()
