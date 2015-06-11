namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Common.Domain
open System.IO
open System.Reflection

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
    
    static let getLocalPath() = 
        (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName

    member public __.RunStarting = runStarting.Publish
    member public __.RunEnded = runEnded.Publish
    member public __.OnRunError = onRunError.Publish
    member public __.RunStepStarting = runStepStarting.Publish
    member public __.OnRunStepError = onRunStepError.Publish
    member public __.RunStepEnded = runStepEnded.Publish
    
    static member public makeRunData startTime solutionPath = 
        { startParams = { startTime = startTime
                          testHostPath = Path.Combine(() |> getLocalPath, "TddStud10.TestHost.exe") |> FilePath
                          solutionPath = solutionPath
                          solutionSnapshotPath = PathBuilder.makeSlnSnapshotPath solutionPath
                          solutionBuildRoot = PathBuilder.makeSlnBuildRoot solutionPath }
          testsPerAssembly = None
          sequencePoints = None
          codeCoverageResults = None
          executedTests = None }
    
    member public __.Start(startTime, solutionPath) = 
        (* NOTE: Need to ensure the started/errored/ended events go out no matter what*)
        let rd = RunExecutor.makeRunData startTime solutionPath
        Common.safeExec (fun () -> runStarting.Trigger(rd))
        let rses = 
            { onStart = runStepStarting
              onError = onRunStepError
              onFinish = runStepEnded }
        
        let rd, err = runSteps |> Seq.fold (executeStep host rses) (rd, None)
        match err with
        | None -> ()
        | Some e -> Common.safeExec (fun () -> onRunError.Trigger(e))
        Common.safeExec (fun () -> runEnded.Trigger(rd))
        rd, err
    
    static member public Create host runSteps stepWrapper = new RunExecutor(host, runSteps, stepWrapper)
