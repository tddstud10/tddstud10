namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Engine.Diagnostics
open System.IO

module PathHelpers = 
    let snapShotRoot = "d:\\tddstud10"
    
    let makeSlnParentDirName slnPath = 
        match Path.GetFileName(Path.GetDirectoryName(slnPath)) with
        | "" -> Path.GetFileNameWithoutExtension(slnPath)
        | dn -> dn
    
    let makeSlnSnapshotPath (FilePath slnPath) = 
        let slnFileName = Path.GetFileName(slnPath)
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName, slnFileName))
    
    let makeSlnBuildRoot (FilePath slnPath) = 
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName + ".out"))

type public RunExecutor(runSteps : RunSteps, stepWrapper : RunStepFunc -> RunStepFunc) = 
    let runStarting = new Event<unit>()
    let runEnded = new Event<unit>()
    let runErrored = new Event<Exception>()
    
    let executeStep (host : IRunExecutorHost) (acc, err) e = 
        match err with
        | Some _ -> acc, err
        | None -> 
            if (host.CanContinue()) then 
                try 
                    (stepWrapper (e.func)) host acc, err
                with ex -> acc, Some ex
            else acc, Some(new OperationCanceledException() :> Exception)
    
    member private this.runSteps = runSteps
    member public this.RunStarting = runStarting.Publish
    member public this.RunEnded = runEnded.Publish
    member public this.RunErrored = runErrored.Publish
    member public this.Start(host : IRunExecutorHost, startTime : DateTime, solutionPath : FilePath) = 
        async { 
            runStarting.Trigger()
            let runData = 
                { startTime = startTime
                  solutionPath = solutionPath
                  solutionSnapshotPath = PathHelpers.makeSlnSnapshotPath solutionPath
                  solutionBuildRoot = PathHelpers.makeSlnBuildRoot solutionPath
                  sequencePoints = None
                  discoveredUnitTests = None
                  codeCoverageResults = None
                  executedTests = None }
            
            let rd, err = runSteps |> Array.fold (executeStep host) (runData, None)
            match err with
            | None -> ()
            | Some e -> runErrored.Trigger(e)
            runEnded.Trigger()
            return rd, err
        }
