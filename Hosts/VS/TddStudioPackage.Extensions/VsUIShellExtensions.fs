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
type public VsUIShellExtensions = 
    [<Extension>]
    static member public DisplayMessageBox(uiShell : IVsUIShell , title : string, text : string) : MessageBoxResult = 
        let mutable clsid : Guid = Guid.Empty
        let mutable result : int = 0
        ErrorHandler.ThrowOnFailure(
            uiShell.ShowMessageBox(
                0u,
                &clsid,
                title,
                text,
                String.Empty,
                0u,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                &result)) |> ignore

        MessageBoxResult.OK
