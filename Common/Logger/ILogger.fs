namespace R4nd0mApps.TddStud10.Logger

open System

type ILogger = 
    abstract LogInfo : string * [<ParamArray>] args:obj [] -> unit
    abstract LogWarn : string * [<ParamArray>] args:obj [] -> unit
    abstract LogError : string * [<ParamArray>] args:obj [] -> unit
    abstract logInfof : Printf.StringFormat<'c, unit> -> 'c
    abstract logWarnf : Printf.StringFormat<'c, unit> -> 'c
    abstract logErrorf : Printf.StringFormat<'c, unit> -> 'c
