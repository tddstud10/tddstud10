namespace R4nd0mApps.TddStud10.Logger

open Microsoft.Diagnostics.Tracing
open R4nd0mApps.TddStud10
open System

[<Sealed>]
[<EventSource(Name = Constants.EtwProviderNameAllLogs)>]
type internal WindowsLogger() = 
    inherit EventSource()
    static let i = new WindowsLogger()
    static member public I = i
    
    [<Event(1, Level = EventLevel.Informational)>]
    member private __.LogInfo(message : string) = base.WriteEvent(1, message)
    
    [<Event(2, Level = EventLevel.Warning)>]
    member private __.LogWarn(message : string) = base.WriteEvent(2, message)
    
    [<Event(3, Level = EventLevel.Error)>]
    member private __.LogError(message : string) = base.WriteEvent(3, message)
    
    [<NonEvent>]
    static member private InvokeLogf level logFn = 
        let l = 
            if WindowsLogger.I.IsEnabled(level, EventKeywords.All) then logFn
            else ignore
        Printf.ksprintf l
    
    interface ILogger with
        
        [<NonEvent>]
        member x.LogInfo(format, args) = 
            if (base.IsEnabled(EventLevel.Informational, EventKeywords.All)) then x.LogInfo(String.Format(format, args))
        
        [<NonEvent>]
        member x.LogWarn(format, args) = 
            if (base.IsEnabled(EventLevel.Warning, EventKeywords.All)) then x.LogWarn(String.Format(format, args))
        
        [<NonEvent>]
        member x.LogError(format, args) = 
            if (base.IsEnabled(EventLevel.Error, EventKeywords.All)) then x.LogError(String.Format(format, args))
        
        [<NonEvent>]
        member __.logInfof format = WindowsLogger.InvokeLogf EventLevel.Informational WindowsLogger.I.LogInfo format
        
        [<NonEvent>]
        member __.logWarnf format = WindowsLogger.InvokeLogf EventLevel.Warning WindowsLogger.I.LogWarn format
        
        [<NonEvent>]
        member __.logErrorf format = WindowsLogger.InvokeLogf EventLevel.Error WindowsLogger.I.LogError format
