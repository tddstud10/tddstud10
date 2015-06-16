module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncBehaviors

open System.Diagnostics
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.Diagnostics

let eventsPublisher f = 
    fun h sp i { onStart = se; onError = ee; onFinish = fe } -> 
        Common.safeExec (fun () -> 
            se.Trigger({ sp = sp
                         info = i }))
        let rsr = 
            try 
                f h sp i { onStart = se
                           onError = ee
                           onFinish = fe }
            with ex -> 
                { status = Aborted
                  runData = NoData
                  addendum = ExceptionData ex }
        if rsr.status <> Succeeded then 
            Common.safeExec (fun () -> 
                ee.Trigger({ sp = sp
                             info = i
                             rsr = rsr }))
        Common.safeExec (fun () -> 
            fe.Trigger({ sp = sp
                         info = i
                         rsr = rsr }))
        if rsr.status <> Succeeded then raise (RunStepFailedException rsr)
        rsr

let stepTimer (f : RunStepFunc) = 
    fun h sp i es -> 
        let sw = Stopwatch()
        sw.Start()
        try 
            f h sp i es
        finally
            let s = sw.Elapsed.ToString("mm\:ss\.ffff")
            Logger.logInfof "[--] Step %A completed in %A" i.name s

let stepLogger (f : RunStepFunc) = 
    fun h sp i es -> 
        Logger.logInfof "[--> Starting step: %A" i.name
        try 
            try 
                f h sp i es
            with ex -> 
                Logger.logErrorf "[**> Exception thrown in step: %A. Exception %s" i.name (ex.ToString())
                reraise()
        finally
            Logger.logInfof "<--] Finishing step: %A" i.name
