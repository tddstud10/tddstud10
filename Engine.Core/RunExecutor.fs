namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.Diagnostics
open System.Collections.Generic

type public RunExecutor private (host : IRunExecutorHost, runSteps : RunSteps, stepWrapper : RunStepFuncWrapper) = 
    let runStarting = new Event<RunData>()
    let runEnded = new Event<RunData>()
    let onRunError = new Event<Exception>()
    let runStepStarting = new Event<RunStepEventArg>()
    let onRunStepError = new Event<RunStepEndEventArg>()
    let runStepEnded = new Event<RunStepEndEventArg>()
    
    let executeStep (host : IRunExecutorHost) events (acc, err) e = 
        match err with
        | Some _ -> acc, err
        | None -> 
            if (host.CanContinue()) then 
                try 
                    let rsr = (e.func |> stepWrapper) host e.name e.kind events acc
                    rsr.runData, err
                with 
                | RunStepFailedException(rsr) as rsfe -> rsr.runData, Some rsfe 
                | ex -> acc, Some ex
            else acc, Some(new OperationCanceledException() :> Exception)
    
    member private this.host = host
    member private this.runSteps = runSteps
    member public this.RunStarting = runStarting.Publish
    member public this.RunEnded = runEnded.Publish
    member public this.OnRunError = onRunError.Publish
    member public this.RunStepStarting = runStepStarting.Publish
    member public this.OnRunStepError = onRunStepError.Publish
    member public this.RunStepEnded = runStepEnded.Publish
    
    static member public makeRunData startTime solutionPath = 
        { startTime = startTime
          solutionPath = solutionPath
          solutionSnapshotPath = PathBuilder.makeSlnSnapshotPath solutionPath
          solutionBuildRoot = PathBuilder.makeSlnBuildRoot solutionPath
          testsPerAssembly = new Dictionary<_, _>()
          sequencePoints = None
          codeCoverageResults = None
          executedTests = None }
    
    member public this.Start(startTime, solutionPath) = 
        (* NOTE: Need to ensure the started/errored/ended events go out no matter what*)
        let rd = RunExecutor.makeRunData startTime solutionPath
        Common.safeExec (fun () -> runStarting.Trigger(rd))
        let rses = 
            { onStart = runStepStarting
              onError = onRunStepError
              onFinish = runStepEnded }
        
        let rd, err = runSteps |> Seq.fold (executeStep this.host rses) (rd, None)
        match err with
        | None -> ()
        | Some e -> Common.safeExec (fun () -> onRunError.Trigger(e))
        Common.safeExec (fun () -> runEnded.Trigger(rd))
        rd, err
    
    static member public Create host runSteps stepWrapper = new RunExecutor(host, runSteps, stepWrapper)
