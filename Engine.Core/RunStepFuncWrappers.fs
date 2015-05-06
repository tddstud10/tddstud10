module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncWrappers

open System.Diagnostics
open R4nd0mApps.TddStud10.Engine.Diagnostics

// Timer
let private runStepFuncTimer f = 
    fun h n es rd -> 
        let sw = Stopwatch()
        sw.Start()
        try 
            f h n es rd
        finally
            let s = sw.Elapsed.ToString("mm\.ss\.ff")
            Logger.logInfof "Step %A completed in %s" n s

// Logger
let private runStepFuncLogger f = 
    fun h n es rd -> 
        Logger.logInfof "Starting step: %A" n
        try 
            try 
                f h n es rd
            with ex -> 
                Logger.logErrorf "Exception thrown in step: %A. Exception %s" n (ex.ToString())
                reraise()
        finally
            Logger.logInfof "Finishing step: %A" n

// Event publisher
let private runStepFuncEventsPublisher f = 
    fun h n (e : RunStepEvent, s : RunStepEvent) rd -> 
        s.Trigger(n, rd)
        try 
            f h n (e, s) rd
        finally
            e.Trigger(n, rd)

// Combined
let CombinedWrapper f = 
    f
    |> runStepFuncEventsPublisher
    |> runStepFuncLogger
    |> runStepFuncTimer
