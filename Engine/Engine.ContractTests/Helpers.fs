namespace R4nd0mApps.TddStud10.Engine.Core

module Helpers = 
    open Newtonsoft.Json
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10.Engine
    open R4nd0mApps.TddStud10.Engine.Core
    open System
    open System.IO
    open System.Reflection
    open System.Text.RegularExpressions
    open System.Threading
    
    let binRoot = 
        Assembly.GetExecutingAssembly().CodeBase
        |> fun cb -> (new Uri(cb)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName
    
    let getTestProjectsRoot testProject = 
        [ Path.GetFullPath
              (Path.Combine(binRoot, @"..\..\..\paket-files\github.com\parthopdas\tddstud10-testprojects\e2e"))
          Path.GetFullPath(Path.Combine(binRoot, @"..\paket-files\github.com\parthopdas\tddstud10-testprojects\e2e")) ]
        |> List.map (fun it -> Path.Combine(it, testProject))
        |> List.find File.Exists
    
    let createRunnerAndDS() = 
        let host = 
            { new IRunExecutorHost with
                  member __.CanContinue() = true
                  member __.HostVersion = HostVersion.VS2015
                  member __.RunStateChanged(_) = () }
        
        let ds = DataStore()
        let ids = ds :> IDataStore
        let r = TddStud10Runner.Create host (Engine.CreateRunSteps(Func<_, _>(ids.FindTest)))
        let es = ResizeArray<obj>()
        r.AttachHandlers (Handler(fun _ -> es.Add)) (Handler(fun _ ea -> 
                                                         es.Add(ea)
                                                         ids.UpdateRunStartParams(ea))) (Handler(fun _ -> es.Add)) 
            (Handler(fun _ ea -> es.Add(ea.sp, ea.info))) (Handler(fun _ ea -> 
                                                               es.Add(ea.sp, ea.info)
                                                               ids.UpdateData(ea.rsr.runData))) 
            (Handler(fun _ ex -> es.Add(ex.Message))) (Handler(fun _ -> es.Add))
        r, ds, es
    
    let cfg = JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
    
    let normalizeEngineOutput es (ds : DataStore) = 
        [ es :> obj
          ds.RunStartParams :> obj
          (ds.TestCases.ToArray() |> Array.sortBy (fun it -> it.Key.ToString())) :> obj
          (ds.SequencePoints.ToArray() |> Array.sortBy (fun it -> it.Key.ToString())) :> obj
          (ds.TestResults.ToArray() |> Array.sortBy (fun it -> it.Key.ToString())) :> obj
          (ds.TestFailureInfo.ToArray() |> Array.sortBy (fun it -> it.Key.ToString())) :> obj
          (ds.CoverageInfo.ToArray()
           |> Array.collect 
                  (fun kv -> 
                  kv.Value.ToArray() |> Array.map (fun v -> (kv.Key.methodId.mdTokenRid, kv.Key.uid), v.testId))
           |> Array.sortBy (fun (um, tid : TestId) -> sprintf "%O.%O" um tid)) :> obj ]
    
    let normalizeJsonDoc (binRoot : string) (root : string) = 
        let regexReplace (p : string, r : string) s = Regex.Replace(s, p, r, RegexOptions.IgnoreCase ||| RegexOptions.Multiline)
        [ @"[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?", "<GUID>"
          @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]*Z", "<DATETIME>"
          @"\""ErrorMessage\""\: \""FsCheck\.Xunit\.PropertyFailedException \: .*Falsifiable, .*\""", @"""ErrorMessage"": ""FsCheck.Xunit.PropertyFailedException : Falsifiable..."""
          binRoot.Replace(@"\", @"\\\\"), "<binroot>"
          root.Replace(@"\", @"\\\\"), "<root>" ]
        |> List.foldBack regexReplace
    
    let toJson o = JsonConvert.SerializeObject(o, Formatting.Indented, cfg)
    
    let runEngine sln props = 
        let ssr = sprintf @"%s\%O" binRoot (Guid.NewGuid())
        try 
            let r, ds, es = createRunnerAndDS()
            let cfg = EngineConfig(SnapShotRoot = ssr, AdditionalMSBuildProperties = props)
            let testProject = getTestProjectsRoot sln
            r.StartAsync cfg (DateTime.UtcNow.AddMinutes(-1.0)) (testProject |> FilePath) (CancellationToken())
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            normalizeEngineOutput es ds |> toJson, testProject
        finally
            if Directory.Exists ssr then Directory.Delete(ssr, true)
