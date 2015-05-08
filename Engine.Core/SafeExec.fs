module R4nd0mApps.TddStud10.Engine.Core.Common

open R4nd0mApps.TddStud10.Engine.Diagnostics

let safeExec (f : unit -> unit) = 
    try 
        f()
    with ex -> Logger.logErrorf "Exception thrown: %s." (ex.ToString())
