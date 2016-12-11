module R4nd0mApps.TddStud10.Engine.Core.Common

let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger

let safeExec (f : unit -> unit) = 
    try 
        f()
    with ex -> logger.logErrorf "Exception thrown: %s." (ex.ToString())

let safeExec2 (f : unit -> 'a): 'a option = 
    try 
        () |> f |> Some
    with ex -> 
        logger.logErrorf "Exception thrown: %s." (ex.ToString())
        None
