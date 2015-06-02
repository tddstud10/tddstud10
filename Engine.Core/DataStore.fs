namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain

type DataStoreXXX() = 
    static let instance = Lazy.Create(fun () -> DataStoreXXX())
    let mutable slnPath = FilePath "" // crap
    let mutable testCases = new PerAssemblyTestCases()
    let testCasesUpdated = new Event<PerAssemblyTestCases>()
    
    interface IDataStore with
        member __.TestCasesUpdated : IEvent<PerAssemblyTestCases> = testCasesUpdated.Publish
        
        member __.UpdateData(rsr : RunStepResult) : unit = 
            slnPath <- rsr.runData.solutionPath
            match rsr.name with
            | RunStepName str when str = "Discover Unit Tests" -> 
                match rsr.runData.testsPerAssembly with
                | Some d -> 
                    testCases <- d
                    Common.safeExec (fun () -> testCasesUpdated.Trigger(testCases))
                | None -> ()
            | _ -> ()
        
        member __.GetUnitTestsInDocument(path : _) = 
            testCases.Values
            |> Seq.collect id
            |> Seq.where (fun t -> PathBuilder.arePathsTheSame slnPath path (FilePath t.CodeFilePath))
    
    static member Instance 
        with public get () = instance.Value
