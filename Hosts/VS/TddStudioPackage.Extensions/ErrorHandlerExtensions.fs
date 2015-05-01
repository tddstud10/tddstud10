namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions

open System
open System.Runtime.CompilerServices
open Microsoft.Diagnostics.Tracing
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics  
open Microsoft.VisualStudio.Shell
open System.Windows
open Microsoft.VisualStudio.Shell.Interop
open Microsoft.VisualStudio
      
[<Extension>]
type public ErrorHandlerExtensions = 
    [<Extension>]
    static member public ThrowOnFailure(hr : int) = 
        ErrorHandler.ThrowOnFailure(hr)
