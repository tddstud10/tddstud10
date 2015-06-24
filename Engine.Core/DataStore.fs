namespace R4nd0mApps.TddStud10.Engine.Core

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain

(* NOTE: Keep this entity free of intelligence. It just needs to be able to store/retrive data.
   Consumers are reponsible for testing their own intelligence. *)
type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
    let mutable runStartParams = None
    let mutable testCases = PerDocumentLocationTestCases()
    let mutable sequencePoints = PerDocumentSequencePoints()
    let mutable testResults = PerTestIdResults()
    let mutable testFailureInfo = PerDocumentLocationTestFailureInfo()
    let mutable coverageInfo = PerSequencePointIdTestRunId()
    let testCasesUpdated = Event<_>()
    let sequencePointsUpdated = Event<_>()
    let testResultsUpdated = Event<_>()
    let testFailureInfoUpdated = Event<_>()
    let coverageInfoUpdated = Event<_>()
    
    interface IDataStore with
        member __.RunStartParams : RunStartParams option = runStartParams
        member __.TestCasesUpdated : IEvent<_> = testCasesUpdated.Publish
        member __.SequencePointsUpdated : IEvent<_> = sequencePointsUpdated.Publish
        member __.TestResultsUpdated : IEvent<_> = testResultsUpdated.Publish
        member __.TestFailureInfoUpdated : IEvent<_> = testFailureInfoUpdated.Publish
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
        
        member __.FindTest dl : TestCase seq = (dl, testCases) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member __.GetSequencePointsForFile p : SequencePoint seq = 
            (p, sequencePoints) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member __.FindTestFailureInfo dl : TestFailureInfo seq = 
            (dl, testFailureInfo) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member __.GetRunIdsForTestsCoveringSequencePointId spid = 
            (spid, coverageInfo) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member __.GetResultsForTestId tid = (tid, testResults) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
    
    static member Instance 
        with public get () = instance.Value :> IDataStore
