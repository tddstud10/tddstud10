namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Engine.Diagnostics

type public RunExecutor private (host : IRunExecutorHost, runSteps : RunSteps, stepWrapper : RunStepFuncWrapper) = 
    let runStarting = new Event<RunData>()
    let runEnded = new Event<RunData>()
    let onRunError = new Event<Exception>()
    let runStepStarting = new RunStepEvent()
    let runStepEnded = new RunStepEvent()
    
    let safeExec (f : unit -> unit) = 
        try 
            f()
        with ex -> Logger.logErrorf "Exception thrown: %s." (ex.ToString())
    
    let executeStep (host : IRunExecutorHost) events (acc, err) e = 
        match err with
        | Some _ -> acc, err
        | None -> 
            if (host.CanContinue()) then 
                try 
                    (e.func |> stepWrapper) host e.name events acc, err
                with ex -> acc, Some ex
            else acc, Some(new OperationCanceledException() :> Exception)
    
    member private this.host = host
    member private this.runSteps = runSteps
    member public this.RunStarting = runStarting.Publish
    member public this.RunEnded = runEnded.Publish
    member public this.OnRunError = onRunError.Publish
    member public this.RunStepStarting = runStepStarting.Publish
    member public this.RunStepEnded = runStepEnded.Publish
    
    member public this.Start (startTime, solutionPath) = 
        (* NOTE: Need to ensure the started/errored/ended events go out no matter what*)
        let runData = 
            { startTime = startTime
              solutionPath = solutionPath
              solutionSnapshotPath = PathBuilder.makeSlnSnapshotPath solutionPath
              solutionBuildRoot = PathBuilder.makeSlnBuildRoot solutionPath
              sequencePoints = None
              discoveredUnitTests = None
              buildConsoleOutput = None
              codeCoverageResults = None
              executedTests = None
              testConoleOutput = None }
        safeExec (fun () -> runStarting.Trigger(runData))
        let rd, err = runSteps |> Array.fold (executeStep this.host (runStepStarting, runStepEnded)) (runData, None)
        match err with
        | None -> ()
        | Some e -> safeExec (fun () -> onRunError.Trigger(e))
        safeExec (fun () -> runEnded.Trigger(runData))
        rd, err
    
    static member public Create host runSteps stepWrapper = new RunExecutor(host, runSteps, stepWrapper)
