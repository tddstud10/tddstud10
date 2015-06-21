namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Generic
open R4nd0mApps.TddStud10.Common

type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
    let mutable runStartParams = None
    let mutable testCases = PerAssemblyTestCases()
    let mutable sequencePoints = PerDocumentSequencePoints()
    let mutable testResults = PerTestIdResults()
    let mutable testFailureInfo = PerDocumentLocationTestFailureInfo()
    let mutable coverageInfo = PerAssemblySequencePointsCoverage()
    let testCasesUpdated = Event<_>()
    let sequencePointsUpdated = Event<_>()
    let testResultsUpdated = Event<_>()
    let testFailureInfoUpdated = Event<_>()
    let coverageInfoUpdated = Event<_>()
    
    let tryGetValue def f k (d : IDictionary<'TKey, 'TValue>) = 
        let found, trs = k |> d.TryGetValue
        if found && trs <> null then trs |> f
        else def
    
    interface IDataStore with
        member __.RunStartParams : RunStartParams option = runStartParams
        member __.TestCasesUpdated : IEvent<_> = testCasesUpdated.Publish
        member __.SequencePointsUpdated : IEvent<_> = sequencePointsUpdated.Publish
        member __.TestResultsUpdated : IEvent<_> = testResultsUpdated.Publish
        member __.TestFailureInfoUpdated : IEvent<_> = testFailureInfoUpdated.Publish
        
        [<CLIEvent>]
        member __.CoverageInfoUpdated : IEvent<_> = coverageInfoUpdated.Publish
        
        member __.UpdateRunStartParams(rsp : RunStartParams) : unit = runStartParams <- rsp |> Some
        
        member __.UpdateData(rd : RunData) : unit = 
            match rd with
            | NoData -> ()
            | TestCases(tc) -> 
                testCases <- tc
                Common.safeExec (fun () -> testCasesUpdated.Trigger(testCases))
            | SequencePoints(sp) -> 
                sequencePoints <- sp
                Common.safeExec (fun () -> sequencePointsUpdated.Trigger(sequencePoints))
            | TestRunOutput(tr, tfi, ci) -> 
                testResults <- tr
                Common.safeExec (fun () -> testResultsUpdated.Trigger(testResults))
                testFailureInfo <- tfi
                Common.safeExec (fun () -> testFailureInfoUpdated.Trigger(testFailureInfo))
                coverageInfo <- ci
                Common.safeExec (fun () -> coverageInfoUpdated.Trigger(coverageInfo))
        
        member __.FindTest assembly document line : TestCase option = 
            let findTest assembly document (DocumentCoordinate line) rsp = 
                (assembly, testCases) 
                ||> tryGetValue None 
                        (fun ts -> 
                        ts 
                        |> Seq.tryFind 
                               (fun t -> 
                               t.LineNumber = line 
                               && (PathBuilder.arePathsTheSame rsp.solutionPath document (FilePath t.CodeFilePath))))
            runStartParams |> Option.bind (findTest assembly document line)
        
        member self.FindTest2 document line : TestCase seq = 
            testCases.Keys |> Seq.choose (fun a -> (self :> IDataStore).FindTest a document line)
        // NOTE: Not tested
        member __.GetAllFiles() : FilePath seq = upcast sequencePoints.Keys
        // NOTE: Not tested
        member __.GetAllSequencePoints() : SequencePoint seq = sequencePoints.Values |> Seq.collect id
        // NOTE: Not tested
        member __.GetSequencePointsForFile p : SequencePoint seq = 
            (p, sequencePoints) ||> tryGetValue Seq.empty (fun v -> v :> seq<_>)
        
        // NOTE: Not tested
        member __.FindTestRunsCoveringSequencePoint sp : TestRunId seq = 
            coverageInfo.Values
            |> Seq.collect id
            // TODO: Why can we not compare the sp.id itself?
            |> Seq.filter (fun spc -> spc.sequencePointId.methodId = sp.id.methodId)
            |> Seq.map (fun spc -> spc.testRunId)
        
        // NOTE: Not tested
        member __.FindTestResults tid : TestRunResult seq = 
            (tid, testResults) ||> tryGetValue Seq.empty (fun v -> v :> seq<_>)
    
    static member Instance 
        with public get () = instance.Value :> IDataStore
