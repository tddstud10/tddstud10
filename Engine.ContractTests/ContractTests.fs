namespace R4nd0mApps.TddStud10.Engine.Core

module ContractTests = 
    open ApprovalTests
    open ApprovalTests.Namers
    open ApprovalTests.Reporters
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10.Engine.Core
    open System
    open System.IO
    open System.Threading
    open Xunit
    
    let ``Test Data - E2E Run for Project`` : obj array seq = 
        [| @"CSXUnit1xNUnit3x.NET20\CSXUnit1xNUnit3x.sln"
           @"VBXUnit1xNUnit2x.NET40\VBXUnit1xNUnit2x.sln"
           @"FSXUnit2xNUnit2x.NET45\FSXUnit2xNUnit2x.sln" |] |> Seq.map (fun a -> [| box a |])
    
    [<UseReporter(typeof<DiffReporter>)>]
    [<UseApprovalSubdirectory("approvals")>]
    [<Theory>]
    [<MemberData("Test Data - E2E Run for Project")>]
    let ``E2E Run for Project`` (sln : string) = 
        use __ = ApprovalResults.ForScenario(Path.GetDirectoryName(sln))
        let ssr = sprintf @"%s\%O" Helpers.binRoot (Guid.NewGuid())
        try 
            let r, ds, es = Helpers.createRunnerAndDS()
            let cfg = EngineConfig(SnapShotRoot = ssr)
            let testProject = Helpers.getTestProjectsRoot sln
            r.StartAsync cfg (DateTime.UtcNow.AddMinutes(-1.0)) (testProject |> FilePath) (CancellationToken())
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            Approvals.VerifyAll
                (Helpers.runStateToJson es ds, "DataStore Entity", 
                 Func<_, _>(Helpers.normalizeJsonDoc Helpers.binRoot (Path.GetDirectoryName(testProject))))
        finally
            Directory.Delete(ssr, true)
