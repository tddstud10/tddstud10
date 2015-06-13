namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel

type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
    let mutable runStartParams = None
    let mutable testCases = PerAssemblyTestCases()
    let testCasesUpdated = Event<PerAssemblyTestCases>()
    
    interface IDataStore with
        member __.RunStartParams : RunStartParams option = runStartParams
        member __.TestCasesUpdated : IEvent<PerAssemblyTestCases> = testCasesUpdated.Publish
        
        member __.UpdateData(rsr : RunStepResult) : unit = 
            runStartParams <- rsr.runData.startParams |> Some
            match rsr.name with
            | RunStepName str when str = "Discover Unit Tests" -> 
                match rsr.runData.testsPerAssembly with
                | Some d -> 
                    testCases <- d
                    Common.safeExec (fun () -> testCasesUpdated.Trigger(testCases))
                | None -> ()
            | _ -> ()
        
        member __.FindTestByDocumentAndLineNumber path (DocumentCoordinate line) : TestCase option = 
            runStartParams |> Option.bind (fun rsp -> 
                                  testCases.Values
                                  |> Seq.collect id
                                  |> Seq.where 
                                         (fun t -> 
                                         PathBuilder.arePathsTheSame rsp.solutionPath path (FilePath t.CodeFilePath))
                                  |> Seq.tryFind (fun t -> t.LineNumber = line))
    
    static member Instance 
        with public get () = instance.Value :> IDataStore
