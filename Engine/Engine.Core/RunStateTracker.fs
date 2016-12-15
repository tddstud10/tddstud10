namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Common.Domain

type RunStateTracker() = 
    let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger

    let mutable state = Initial
    let runStateChanged = new Event<_>()

    let logAndReturnBack s ev =
        logger.logErrorf "Run Tracker State Machine: Cannot handle event '%A' in state '%A'" ev s
        s
            
    let transitionState = 
        function 
        | _, RunStarting -> Initial

        | s, RunError(RunStepFailedException ({status = Failed; addendum = _ })) -> s
        | _, RunError(_) -> EngineError
        
        | _, RunStepError(_, Aborted) -> EngineErrorDetected
        | _, RunStepEnded(_, Aborted) -> EngineError

        | Initial, RunStepStarting(Build) -> FirstBuildRunning
        | Initial as s, ev -> logAndReturnBack s ev 

        | EngineErrorDetected as s, ev -> logAndReturnBack s ev 
        
        | EngineError as s, ev -> logAndReturnBack s ev
        
        | FirstBuildRunning, RunStepEnded(Build, Succeeded) -> BuildPassed
        | FirstBuildRunning, RunStepError(Build, Failed) -> BuildFailureDetected
        | FirstBuildRunning as s, ev -> logAndReturnBack s ev
        
        | BuildFailureDetected, RunStepEnded(Build, Failed) -> BuildFailed
        | BuildFailureDetected as s, ev -> logAndReturnBack s ev
        
        | BuildFailed as s, ev -> logAndReturnBack s ev
        
        | TestFailureDetected, RunStepEnded(Test, Failed) -> TestFailed
        | TestFailureDetected as s, ev -> logAndReturnBack s ev
        
        | TestFailed as s, ev -> logAndReturnBack s ev
        
        | BuildRunning, RunStepError(Build, Failed) -> BuildFailureDetected
        | BuildRunning, RunStepEnded(Build, Succeeded) -> BuildPassed
        | BuildRunning as s, ev -> logAndReturnBack s ev
        
        | BuildPassed, RunStepStarting(Build) -> BuildRunning
        | BuildPassed, RunStepStarting(Test) -> TestRunning
        | BuildPassed as s, ev -> logAndReturnBack s ev
        
        | TestRunning, RunStepError(Test, Failed) -> TestFailureDetected
        | TestRunning, RunStepEnded(Test, Succeeded) -> TestPassed
        | TestRunning as s, ev -> logAndReturnBack s ev
        
        | TestPassed as s, ev -> logAndReturnBack s ev
    
    let transitionStateAndRaiseEvent ev = 
        let oldState = state
        state <- transitionState (state, ev)
        logger.logInfof "Run Tracker State Machine: Trasition (%A, %A) -> %A" oldState ev state
        Common.safeExec (fun () -> runStateChanged.Trigger(state))
    
    member __.State = state
    member public __.RunStateChanged = runStateChanged.Publish
    member public __.OnRunStarting(_ : RunStartParams) = transitionStateAndRaiseEvent RunStarting
    member public __.OnRunStepStarting(ea : RunStepStartingEventArg) = transitionStateAndRaiseEvent (RunStepStarting ea.info.kind)
    member public __.OnRunStepError(ea : RunStepErrorEventArg) = 
        transitionStateAndRaiseEvent (RunStepError(ea.info.kind, ea.rsr.status))
    member public __.OnRunStepEnd(ea : RunStepEndedEventArg) = 
        transitionStateAndRaiseEvent (RunStepEnded(ea.info.kind, ea.rsr.status))
    member public __.OnRunError(ea : Exception) = transitionStateAndRaiseEvent (RunError ea)
    member public __.OnRunEnd(_ : RunStartParams) = ()
