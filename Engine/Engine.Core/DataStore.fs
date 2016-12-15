namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain

(* NOTE: Keep this entity free of intelligence. It just needs to be able to store/retrive data.
   Consumers are reponsible for testing their own intelligence. *)
type DataStore() = 
    static let instance = Lazy.Create(fun () -> DataStore())
    let testCasesUpdated = Event<_>()
    let sequencePointsUpdated = Event<_>()
    let testResultsUpdated = Event<_>()
    let testFailureInfoUpdated = Event<_>()
    let coverageInfoUpdated = Event<_>()
    member val RunStartParams = None with get, set
    member val TestCases = PerDocumentLocationDTestCases() with get, set
    member val SequencePoints = PerDocumentSequencePoints() with get, set
    member val TestResults = PerTestIdDResults() with get, set
    member val TestFailureInfo = PerDocumentLocationTestFailureInfo() with get, set
    member val CoverageInfo = PerSequencePointIdTestRunId() with get, set
    member private x.UpdateData = function
        | NoData -> ()
        | TestCases(tc) -> 
            x.TestCases <- tc
            Common.safeExec (fun () -> testCasesUpdated.Trigger(x.TestCases))
        | SequencePoints(sp) -> 
            x.SequencePoints <- sp
            Common.safeExec (fun () -> sequencePointsUpdated.Trigger(x.SequencePoints))
        | TestRunOutput(tr, tfi, ci) -> 
            x.TestResults <- tr
            Common.safeExec (fun () -> testResultsUpdated.Trigger(x.TestResults))
            x.TestFailureInfo <- tfi
            Common.safeExec (fun () -> testFailureInfoUpdated.Trigger(x.TestFailureInfo))
            x.CoverageInfo <- ci
            Common.safeExec (fun () -> coverageInfoUpdated.Trigger(x.CoverageInfo))
        
    interface IDataStore with
        member x.RunStartParams : RunStartParams option = x.RunStartParams
        member __.TestCasesUpdated : IEvent<_> = testCasesUpdated.Publish
        member __.SequencePointsUpdated : IEvent<_> = sequencePointsUpdated.Publish
        member __.TestResultsUpdated : IEvent<_> = testResultsUpdated.Publish
        member __.TestFailureInfoUpdated : IEvent<_> = testFailureInfoUpdated.Publish
        member __.CoverageInfoUpdated : IEvent<_> = coverageInfoUpdated.Publish
        member x.UpdateRunStartParams(rsp : RunStartParams) : unit = x.RunStartParams <- rsp |> Some
                
        member x.UpdateData(rd : RunData) : unit = x.UpdateData rd

        member x.ResetData() =
            PerDocumentSequencePoints() |> SequencePoints |> x.UpdateData
            PerDocumentLocationDTestCases() |> TestCases |> x.UpdateData
            (PerTestIdDResults(), PerDocumentLocationTestFailureInfo(), PerSequencePointIdTestRunId()) |> TestRunOutput |> x.UpdateData
        
        member x.FindTest dl : DTestCase seq = (dl, x.TestCases) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member x.GetSequencePointsForFile p : SequencePoint seq = 
            (p, x.SequencePoints) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member x.FindTestFailureInfo dl : TestFailureInfo seq = 
            (dl, x.TestFailureInfo) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member x.GetRunIdsForTestsCoveringSequencePointId spid = 
            (spid, x.CoverageInfo) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
        member x.GetResultsForTestId tid = (tid, x.TestResults) ||> Dict.tryGetValue Seq.empty (fun v -> v :> seq<_>)
    
    static member Instance 
        with public get () = instance.Value :> IDataStore
