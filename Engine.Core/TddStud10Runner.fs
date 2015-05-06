namespace R4nd0mApps.TddStud10.Engine.Core

open System

(* NOTE: 
   This should just be a wire up class. No business logic at all. 
   Hence not TDDed. *)
type public TddStud10Runner private (re, agent) = 
    member private t.re = re
    member private t.agent = agent
    
    static member public Create host runSteps stepWrapper = 
        let rsw = RunStepFuncWrappers.CombinedWrapper
        let re = RunExecutor.Create host runSteps rsw
        let agent = new StaleMessageIgnoringAgent<DateTime * FilePath>(re.Start >> ignore)
        new TddStud10Runner(re, agent)
    
    member public t.AttachHandlers sh ssh seh erh eh = 
        t.re.RunStarting.AddHandler(sh)
        t.re.RunStepStarting.AddHandler(ssh)
        t.re.RunStepEnded.AddHandler(seh)
        t.re.OnRunError.AddHandler(erh)
        t.re.RunEnded.AddHandler(eh)
    
    member public t.DetachHandlers eh erh seh ssh sh = 
        t.re.RunEnded.RemoveHandler(eh)
        t.re.OnRunError.RemoveHandler(erh)
        t.re.RunStepEnded.RemoveHandler(seh)
        t.re.RunStepStarting.RemoveHandler(ssh)
        t.re.RunStarting.RemoveHandler(sh)
