namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel

type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
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
        
        member __.FindTestByDocumentAndLineNumber path (DocumentCoordinate line) : TestCase option = 
            testCases.Values
            |> Seq.collect id
            |> Seq.where (fun t -> PathBuilder.arePathsTheSame slnPath path (FilePath t.CodeFilePath))
            |> Seq.tryFind (fun t -> t.LineNumber = line)
    
    static member Instance 
        with public get () = instance.Value

#if DONT_COMPILE
UpdateData
- fire event with expected data
- doesnt crash one xception 

FindTestByDocumentAndLineNumber
- none - after an update
- some - after an update
- after fire event doesnt have old data, works off new data
- slnpath test

#endif