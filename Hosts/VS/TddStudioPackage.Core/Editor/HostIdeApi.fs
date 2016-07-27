namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open R4nd0mApps.TddStud10.Common.Domain

type HostIdeApi = 
    { GotoTest : SequencePoint * DTestCase -> unit
      DebugTest : SequencePoint * DTestCase -> unit
      RunTest : SequencePoint * DTestCase -> unit
      IdeInDebugMode : unit -> bool }

[<AutoOpen>]
module HostIdeApiExtensions = 
    open Microsoft.VisualStudio.Shell.Interop
    open R4nd0mApps.TddStud10.Engine.Core
    open R4nd0mApps.TddStud10.Engine.Core.Common
    open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core
    open System
    open System.Collections.Concurrent
    open System.IO
    
    let private launch (debugger : IVsDebugger3) (FilePath exe) args = 
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
    
    let private setBreakPoint (dte : EnvDTE.DTE) (FilePath file) (DocumentCoordinate line) = dte.Debugger.Breakpoints.Add(null, file, line)
    
    let gotoTest (dte : EnvDTE.DTE) = 
        let f (_, tc) () = 
            let ({ CodeFilePath = (FilePath file); LineNumber = (DocumentCoordinate line) }) = tc
            dte.ItemOperations.OpenFile(file, EnvDTE.Constants.vsViewKindTextView) |> ignore
            (dte.ActiveDocument.Selection :?> EnvDTE.TextSelection).GotoLine(line, false)
        f >> safeExec
    
    let debugTest dte dbg = 
        let f (sp : SequencePoint, tc) () = 
            setBreakPoint dte sp.document sp.startLine |> ignore
            let tpa = PerDocumentLocationDTestCases()
            let bag = ConcurrentBag<DTestCase>()
            bag.Add(tc)
            tpa.TryAdd({ document = sp.document
                         line = sp.startLine }, bag)
            |> ignore
            tpa.Serialize(DataStore.Instance.RunStartParams.Value.DataFiles.DiscoveredUnitDTestsStore)
            DataStore.Instance.RunStartParams 
            |> Option.fold 
                (fun () e -> launch dbg e.TestHostPath (TestHost.buildCommandLine "execute" e)) 
                (Logger.logErrorf "DataStore.Instance.RunStartParams not yet set")
        f >> safeExec
    
    let runTest = 
        let f (_, _) () = 
            // Not implemented yet. As we dont have infra to put the results back on the IDE.
            ()
        f >> safeExec
    
    let ideInDebugMode (dte : EnvDTE.DTE) = 
        let f () () = 
            dte.Mode = EnvDTE.vsIDEMode.vsIDEModeDebug
        f >> safeExec2 >> (Option.fold (fun _ -> id) false)
    
    let createHostIdeApi dte dbg = 
        { GotoTest = gotoTest dte
          DebugTest = debugTest dte dbg
          RunTest = runTest
          IdeInDebugMode = ideInDebugMode dte }
