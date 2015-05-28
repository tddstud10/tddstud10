namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions

open System.Runtime.CompilerServices
open Microsoft.VisualStudio
      
[<Extension>]
type public ErrorHandlerExtensions = 
    [<Extension>]
    static member public ThrowOnFailure(hr : int) = 
        ErrorHandler.ThrowOnFailure(hr)
