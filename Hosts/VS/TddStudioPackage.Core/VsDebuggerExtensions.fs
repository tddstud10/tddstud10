namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core

open Microsoft.VisualStudio.Shell.Interop
open System.Runtime.CompilerServices
open System.IO
open System
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

[<Extension>]
type public VsDebuggerExtensions = 
    
    [<Extension>]
    static member public Launch (debugger : IVsDebugger3) exe args = 
        let (targets : VsDebugTargetInfo3 array) = Array.zeroCreate 1
        targets.[0].dlo <- DEBUG_LAUNCH_OPERATION.DLO_CreateProcess |> uint32
        targets.[0].guidLaunchDebugEngine <- Guid("449EC4CC-30D2-4032-9256-EE18EB41B62B")
        targets.[0].bstrExe <- exe
        targets.[0].bstrArg <- args
        targets.[0].bstrCurDir <- exe |> Path.GetDirectoryName
        let (results : VsDebugTargetProcessInfo array) = Array.zeroCreate targets.Length
        debugger.LaunchDebugTargets3(targets.Length |> uint32, targets, results) 
        |> ErrorHandlerExtensions.ThrowOnFailure
        results |> Array.iter (fun r -> Logger.logInfof "%d launched under debugger at %O" r.dwProcessId r.creationTime)
    
    [<Extension>]
    static member public SetBreakPoint (dte : EnvDTE.DTE) file line = 
        dte.Debugger.Breakpoints.Add(null, file, line)
