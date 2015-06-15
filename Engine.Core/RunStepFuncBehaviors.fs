module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncBehaviors

open System.Diagnostics
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.Diagnostics

let eventsPublisher f = 
    fun h sp n k sk { onStart = se; onError = ee; onFinish = fe } -> 
        Common.safeExec (fun () -> se.Trigger({ startParams = sp; name = n; kind = k; subKind = sk }))
        let rss = 
            try 
                f h sp n k sk { onStart = se
                                onError = ee
                                onFinish = fe }
            with ex -> 
                { startParams = sp
                  name = n
                  kind = k
                  subKind = sk
                  status = Aborted
                  runData = NoData
                  addendum = ExceptionData ex }
        if rss.status <> Succeeded then 
            Common.safeExec (fun () -> ee.Trigger({ rsr = rss }))

        Common.safeExec (fun () -> fe.Trigger({ rsr = rss }))

        if rss.status <> Succeeded then 
            raise (RunStepFailedException rss)
        rss

let stepTimer f = 
    fun h sp n k sk es -> 
        let sw = Stopwatch()
        sw.Start()
        try 
            f h sp n k sk es
        finally
            let s = sw.Elapsed.ToString("mm\:ss\.ffff")
            Logger.logInfof "[--] Step %A completed in %s" n s

let stepLogger f = 
    fun h sp n k sk es -> 
        Logger.logInfof "[--> Starting step: %A" n
        try 
            try 
                f h sp n k sk es
            with ex -> 
                Logger.logErrorf "[**> Exception thrown in step: %A. Exception %s" n (ex.ToString())
                reraise()
        finally
            Logger.logInfof "<--] Finishing step: %A" n
