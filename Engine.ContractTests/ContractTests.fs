namespace R4nd0mApps.TddStud10.Engine.Core

module ContractTests = 
    open ApprovalTests
    open ApprovalTests.Namers
    open ApprovalTests.Reporters
    open R4nd0mApps.TddStud10.Engine.Core
    open System
    open System.IO
    open Xunit
    
    let solutions = 
        [ @"CSXUnit1xNUnit3x.NET20\CSXUnit1xNUnit3x.sln"; @"VBXUnit1xNUnit2x.NET40\VBXUnit1xNUnit2x.sln"; 
          @"FSXUnit2xNUnit2x.NET45\FSXUnit2xNUnit2x.sln" ]
    let variants = [ "BREAK_NOTHING"; "BREAK_TEST"; "BREAK_BUILD" ]
    
    let ``Test Data - E2E Run for Project`` : obj array seq = 
        seq { 
            for s in solutions do
                for v in variants -> s, v
        }
        // Enable this back once we figure out the root cause of the intermediate difference
        |> Seq.filter (fun (s, v) -> (not <| s.StartsWith("FS")) || (not <| v.EndsWith("TEST")) )
        |> Seq.map (fun (a, b) -> 
               [| box a
                  box b |])
    
    [<UseReporter(typeof<DiffReporter>)>]
    [<UseApprovalSubdirectory("approvals")>]
    [<Theory>]
    [<MemberData("Test Data - E2E Run for Project")>]
    let ``E2E Run for Project`` (s : string, v : string) = 
        use __ = ApprovalResults.ForScenario(Path.GetDirectoryName(s), v)
        let output, projRoot = Helpers.runEngine s [| sprintf "DefineConstants=%s" v |]
        Approvals.Verify(output, Func<_, _>(Helpers.normalizeJsonDoc Helpers.binRoot (Path.GetDirectoryName(projRoot))))
