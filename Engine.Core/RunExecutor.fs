namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open System

type public RunExecutor private (host : IRunExecutorHost, runSteps : RunSteps, stepWrapper : RunStepFuncWrapper) = 
    let tc = R4nd0mApps.TddStud10.Logger.TelemetryClientFactory.telemetryClient
    let runStarting = new Event<_>()
    let runEnded = new Event<_>()
    let onRunError = new Event<_>()
    let runStepStarting = new Event<_>()
    let onRunStepError = new Event<_>()
    let runStepEnded = new Event<_>()
    
    let executeStep (host : IRunExecutorHost) sp events err e = 
        match err with
        | Some _ -> err
        | None -> 
            if (host.CanContinue()) then 
                try 
                    (e.func |> stepWrapper) host sp e.info events |> ignore
                    err
                with
                | RunStepFailedException(_) as rsfe -> Some rsfe
                | ex -> Some ex
            else Some(new OperationCanceledException() :> _)
    
    member public __.RunStarting = runStarting.Publish
    member public __.RunEnded = runEnded.Publish
    member public __.OnRunError = onRunError.Publish
    member public __.RunStepStarting = runStepStarting.Publish
    member public __.OnRunStepError = onRunStepError.Publish
    member public __.RunStepEnded = runStepEnded.Publish
    
    member public __.Start(cfg, startTime, solutionPath) = 
        (* NOTE: Need to ensure the started/errored/ended events go out no matter what*)
        let rsp = RunStartParams.Create cfg startTime solutionPath
        let op = tc.StartOperation("TddStud10 Run")
        Common.safeExec (fun () -> runStarting.Trigger(rsp))
        let rses = 
            { onStart = runStepStarting
              onError = onRunStepError
              onFinish = runStepEnded }
        
        let err = runSteps |> Seq.fold (executeStep host rsp rses) None
        match err with
        | None -> ()
        | Some e -> Common.safeExec (fun () -> onRunError.Trigger(e))
        Common.safeExec (fun () -> runEnded.Trigger(rsp))
        tc.StopOperation(op)
        rsp, err
    
    static member public Create host runSteps stepWrapper = new RunExecutor(host, runSteps, stepWrapper)
