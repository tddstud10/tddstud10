namespace R4nd0mApps.TddStud10.Logger

open System.Diagnostics

[<Sealed>]
type internal NullLogger() = 
    
    static member private InvokeLogf logFn = 
        let l = logFn
        Printf.ksprintf l
    
    interface ILogger with
        member __.LogInfo(format, args) = Trace.TraceInformation(format, args)
        member __.LogWarn(format, args) = Trace.TraceWarning(format, args)
        member __.LogError(format, args) = Trace.TraceError(format, args)
        member __.logInfof format = NullLogger.InvokeLogf Trace.TraceInformation format
        member __.logWarnf format = NullLogger.InvokeLogf Trace.TraceWarning format
        member __.logErrorf format = NullLogger.InvokeLogf Trace.TraceError format
