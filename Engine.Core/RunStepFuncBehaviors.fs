module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncBehaviors

open System.Diagnostics
open R4nd0mApps.TddStud10.Engine.Diagnostics

let runStepFuncTimer f = 
    fun h n es rd -> 
        let sw = Stopwatch()
        sw.Start()
        try 
            f h n es rd
        finally
            let s = sw.Elapsed.ToString("mm\:ss\.ffff")
            Logger.logInfof "[--] Step %A completed in %s" n s

let runStepFuncLogger f = 
    fun h n es rd -> 
        Logger.logInfof "[--> Starting step: %A" n
        try 
            try 
                f h n es rd
            with ex -> 
                Logger.logErrorf "[**> Exception thrown in step: %A. Exception %s" n (ex.ToString())
                reraise()
        finally
            Logger.logInfof "<--] Finishing step: %A" n

let runStepFuncEventsPublisher f = 
    fun h n (s : RunStepEvent, e : RunStepEvent) rd -> 
        s.Trigger(n, rd)
        try 
            f h n (e, s) rd
        finally
            e.Trigger(n, rd)
