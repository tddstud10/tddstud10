namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core

open System.Runtime.CompilerServices
open Microsoft.VisualStudio
      
[<Extension>]
type public ErrorHandlerExtensions = 

    [<Extension>]
    static member public ThrowOnFailure(hr : int) = 
        let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger
        if ErrorHandler.Failed hr then
            logger.logErrorf "hr = %d. Going to throw up." hr
        ErrorHandler.ThrowOnFailure(hr) |> ignore
