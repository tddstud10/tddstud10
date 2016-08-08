module R4nd0mApps.TddStud10.Engine.Core

// TODO:
// - Test for RunStateChanged
// - Introduce Errors and test w/ runstate
module ContractTests = 
    open ApprovalTests
    open ApprovalTests.Reporters
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10.Engine.Core
    open System
    open System.IO
    open System.Threading
    open Xunit
    
    [<UseReporter(typeof<DiffReporter>)>]
    [<Fact>]
    let ``A sample test``() = 
        let reHost = 
            { new IRunExecutorHost with
                  member __.CanContinue() = true
                  member __.HostVersion = HostVersion.VS2013
                  member __.RunStateChanged(_) = () }
        
        let r = TddStud10Runner.Create reHost (Engine.CreateRunSteps())
        let ds = DataStore() :> IDataStore
        r.AttachHandlers (Handler(fun _ _ -> ())) (Handler(fun _ ea -> ds.UpdateRunStartParams(ea))) 
            (Handler(fun _ _ -> ())) (Handler(fun _ _ -> ())) (Handler(fun _ ea -> ds.UpdateData(ea.rsr.runData))) 
            (Handler(fun _ _ -> ())) (Handler(fun _ _ -> ()))
        let cfg = EngineConfig()
        cfg.SnapShotRoot <- sprintf @"D:\src\gh\tddstud10\Engine.ContractTests\bin\Debug\%O" (Guid.NewGuid())
        let t = 
            r.StartAsync cfg (DateTime.UtcNow.AddMinutes(-1.0)) 
                (FilePath 
                     @"D:\src\gh\tddstud10\AcceptanceTests\AdapterTests\1_VBXUnit1xNUnit2x.NET40\VBXUnit1xNUnit2x.sln") 
                (CancellationToken())
        t.Result |> ignore
        Directory.Delete(cfg.SnapShotRoot, true)
        Approvals.Verify(ds)
