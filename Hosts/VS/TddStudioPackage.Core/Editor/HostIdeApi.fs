namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open R4nd0mApps.TddStud10.Common.Domain

type HostIdeApi = 
    { GotoTest : DTestCase -> unit
      DebugTest : DTestCase -> unit
      RunTest : DTestCase -> unit }

[<AutoOpen>]
module HostIdeApiExtensions = 
    open R4nd0mApps.TddStud10.Engine.Core.Common
    
    let gotoTest (dte : EnvDTE.DTE) = 
        let f tc () = 
            let ({ CodeFilePath = (FilePath file); LineNumber = (DocumentCoordinate line) }) = tc
            dte.ItemOperations.OpenFile(file, EnvDTE.Constants.vsViewKindTextView) |> ignore
            (dte.ActiveDocument.Selection :?> EnvDTE.TextSelection).GotoLine(line, false)
        f >> safeExec
    
    let debugTest tc dte2 = ()
    let runTest tc a = ()
    
    let createHostIdeApi dte dbg = 
        { GotoTest = gotoTest dte
          DebugTest = debugTest dte
          RunTest = runTest dte }
