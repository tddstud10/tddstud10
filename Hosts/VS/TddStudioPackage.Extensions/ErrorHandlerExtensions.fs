namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions

open System.Runtime.CompilerServices
open Microsoft.VisualStudio
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
      
[<Extension>]
type public ErrorHandlerExtensions = 
    [<Extension>]
    static member public ThrowOnFailure(hr : int) = 
        if ErrorHandler.Failed hr then
            Logger.logErrorf "hr = %d. Going to throw up." hr
        ErrorHandler.ThrowOnFailure(hr) |> ignore
