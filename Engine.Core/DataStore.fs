namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel

type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
    // Subscribe to runstart and update these properties
    // Combine all 4 rundata members
    // Change to none
    let mutable slnPath = FilePath "" // crap
    let mutable slnBuildRoot = FilePath "" // crap
    let mutable testCases = new PerAssemblyTestCases()
    let testCasesUpdated = new Event<PerAssemblyTestCases>()
    
    interface IDataStore with
        member __.SolutionBuildRoot: FilePath = 
            slnBuildRoot
        
        member __.TestCasesUpdated : IEvent<PerAssemblyTestCases> = testCasesUpdated.Publish
        
        member __.UpdateData(rsr : RunStepResult) : unit = 
            slnPath <- rsr.runData.solutionPath
            slnBuildRoot <- rsr.runData.solutionBuildRoot
            match rsr.name with
            | RunStepName str when str = "Discover Unit Tests" -> 
                match rsr.runData.testsPerAssembly with
                | Some d -> 
                    testCases <- d
                    Common.safeExec (fun () -> testCasesUpdated.Trigger(testCases))
                | None -> ()
            | _ -> ()
        
        member __.FindTestByDocumentAndLineNumber path (DocumentCoordinate line) : TestCase option = 
            testCases.Values
            |> Seq.collect id
            |> Seq.where (fun t -> PathBuilder.arePathsTheSame slnPath path (FilePath t.CodeFilePath))
            |> Seq.tryFind (fun t -> t.LineNumber = line)
    
    static member Instance 
        with public get () = instance.Value :> IDataStore
