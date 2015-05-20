module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncBehaviors

open System.Diagnostics
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.Diagnostics

let eventsPublisher f = 
    fun h n k { onStart = se; onError = ee; onFinish = fe } rd -> 
        Common.safeExec (fun () -> se.Trigger({name = n; kind = k; runData = rd}))
        let rss = 
            try 
                f h n k { onStart = se
                          onError = ee
                          onFinish = fe } rd
            with ex -> 
                { name = n
                  kind = k
                  status = Aborted
                  addendum = ExceptionData ex
                  runData = rd }
        if rss.status <> Succeeded then 
            Common.safeExec (fun () -> ee.Trigger(rss))

        Common.safeExec (fun () -> fe.Trigger(rss))

        if rss.status <> Succeeded then 
            raise (RunStepFailedException rss)
        rss

let stepTimer f = 
    fun h n k es rd -> 
        let sw = Stopwatch()
        sw.Start()
        try 
            f h n k es rd
        finally
            let s = sw.Elapsed.ToString("mm\:ss\.ffff")
            Logger.logInfof "[--] Step %A completed in %s" n s

let stepLogger f = 
    fun h n k es rd -> 
        Logger.logInfof "[--> Starting step: %A" n
        try 
            try 
                f h n k es rd
            with ex -> 
                Logger.logErrorf "[**> Exception thrown in step: %A. Exception %s" n (ex.ToString())
                reraise()
        finally
            Logger.logInfof "<--] Finishing step: %A" n
