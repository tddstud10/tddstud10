﻿module R4nd0mApps.TddStud10.Engine.Core

module ContractTests = 
    open ApprovalTests
    open ApprovalTests.Namers
    open ApprovalTests.Reporters
    open Newtonsoft.Json
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10.Engine.Core
    open System
    open System.IO
    open System.Reflection
    open System.Text.RegularExpressions
    open System.Threading
    open Xunit
    
    let binRoot = 
        Assembly.GetExecutingAssembly().CodeBase
        |> fun cb -> (new Uri(cb)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName
    
    let getTestProjectsRoot testProject = 
        [ Path.GetFullPath(Path.Combine(binRoot, "..\..\..\AcceptanceTests\AdapterTests"))
          Path.GetFullPath(Path.Combine(binRoot, "..\AcceptanceTests\AdapterTests")) ]
        |> List.map (fun it -> Path.Combine(it, testProject))
        |> List.find File.Exists
    
    let createRunnerAndDS() = 
        let host = 
            { new IRunExecutorHost with
                  member __.CanContinue() = true
                  member __.HostVersion = HostVersion.VS2013
                  member __.RunStateChanged(_) = () }
    
        let ds = DataStore()
        let ids = ds :> IDataStore
        let r = TddStud10Runner.Create host (Engine.CreateRunSteps(Func<_, _>(ids.FindTest)))
        let es = ResizeArray<obj>()
        r.AttachHandlers (Handler(fun _ -> es.Add)) (Handler(fun _ ea -> ids.UpdateRunStartParams(ea))) 
            (Handler(fun _ -> es.Add)) (Handler(fun _ -> es.Add)) (Handler(fun _ ea -> ids.UpdateData(ea.rsr.runData))) 
            (Handler(fun _ -> es.Add)) (Handler(fun _ -> es.Add))
        r, ds, es
    
    let runStateToJson es (ds : DataStore) = 
        let toJson o = JsonConvert.SerializeObject(o, Formatting.Indented)
        [ toJson es
          toJson ds.RunStartParams
          toJson (ds.TestCases.ToArray() |> Array.sortBy (fun it -> it.Key.ToString()))
          toJson (ds.SequencePoints.ToArray() |> Array.sortBy (fun it -> it.Key.ToString()))
          toJson (ds.TestResults.ToArray() |> Array.sortBy (fun it -> it.Key.ToString()))
          toJson (ds.TestFailureInfo.ToArray() |> Array.sortBy (fun it -> it.Key.ToString()))
          toJson (ds.CoverageInfo.ToArray()
                  |> Array.collect 
                         (fun kv -> 
                         kv.Value.ToArray() |> Array.map (fun v -> (kv.Key.methodId.mdTokenRid, kv.Key.uid), v.testId))
                  |> Array.sortBy (fun (um, tid : TestId) -> sprintf "%O.%O" um tid)) ]
    
    let normalizeJsonDoc (binRoot : string) (root : string) = 
        let regexReplace (p : string, r : string) s = Regex.Replace(s, p, r, RegexOptions.IgnoreCase)
        [ @"[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?", "<GUID>"
          @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]*Z", "<DATETIME>"
          binRoot.Replace(@"\", @"\\\\"), "<binroot>"
          root.Replace(@"\", @"\\\\"), "<root>" ]
        |> List.foldBack regexReplace
    
    [<Fact>]
    [<UseReporter(typeof<DiffReporter>)>]
    [<UseApprovalSubdirectory("approvals")>]
    let ``E2E Run for Project``() = 
        let ssr = sprintf @"%s\%O" binRoot (Guid.NewGuid())
        try 
            let r, ds, es = createRunnerAndDS()
            let cfg = EngineConfig(SnapShotRoot = ssr)
            let testProject = getTestProjectsRoot "1_VBXUnit1xNUnit2x.NET40\VBXUnit1xNUnit2x.sln"
            r.StartAsync cfg (DateTime.UtcNow.AddMinutes(-1.0)) (testProject |> FilePath) (CancellationToken())
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            Approvals.VerifyAll
                (runStateToJson es ds, "DataStore Entity", 
                 Func<_, _>(normalizeJsonDoc binRoot (Path.GetDirectoryName(testProject))))
        finally
            Directory.Delete(ssr, true)
