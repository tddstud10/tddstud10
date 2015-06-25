module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.CodeCoverageTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.TestCommon
open Microsoft.VisualStudio.TestPlatform.ObjectModel

let createTB p t = FakeTextBuffer(t, p) :> ITextBuffer
let stubTR1 = { name = "Test Result #1"; outcome = TestOutcome.Passed }
let stubTR2 = { name = "Test Result #2"; outcome = TestOutcome.Failed }

let stubTC1 = 
    { fqn = "FQNTest#1"
      src = "testdll1.dll"
      file = "test1.cpp"
      ln = 100 }

let stubTC2 = 
    { fqn = "FQNTest#2"
      src = "testdll2.dll"
      file = "test2.cpp"
      ln = 200 }

let stubSpidXLineNumber = 3 |> DocumentCoordinate
let stubSpidXTb = createTB "code.cs" """
The 2nd line in the file whose sequence points we are tagging
3rd Line // stubSpid1 and stubSpid2 are here, stubSpidXLineNumber is this line
"""

let stubSpid1 = 
    { methodId = 
          { assemblyId = AssemblyId(Guid.NewGuid())
            mdTokenRid = MdTokenRid 101u }
      uid = 1 }

let stubSpid2 = 
    { methodId = 
          { assemblyId = AssemblyId(Guid.NewGuid())
            mdTokenRid = MdTokenRid 102u }
      uid = 2 }

let getNSSC (DocumentCoordinate ln) (tb : ITextBuffer) = 
    let ss = 
        tb.CurrentSnapshot.Lines
        |> Seq.skip (ln - 1)
        |> Seq.take 1
        |> Seq.map (fun l -> l.Extent)
    NormalizedSnapshotSpanCollection(ss)

let createCCT s tb ta = 
    let ta : SnapshotSnapsToTagSpan<SequencePointTag> = ta
    let ds = DataStore() :> IDataStore
    RunStartParamsExtensions.create DateTime.Now (FilePath s) |> ds.UpdateRunStartParams
    let tmt = CodeCoverageTagger(tb, ta, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    ds, tb, tmt, spy

let createCCT2 (tb : ITextBuffer) (DocumentCoordinate ln) sps = 
    let getspt ln tss (ss : NormalizedSnapshotSpanCollection) = 
        let it = 
            if ss.[0].Start.GetContainingLine().LineNumber = ln - 1 && ss.[0].Snapshot.TextBuffer.FilePath = tb.FilePath then 
                tss
            else []
        it :> seq<TagSpan<_>>
    
    let nssc = tb |> getNSSC (DocumentCoordinate ln)
    let tss = sps |> List.map (fun sp -> TagSpan<SequencePointTag>(nssc.[0], { sp = sp }))
    let ds, _, cct, _ = createCCT "a.sln" tb (getspt ln tss)
    ds, cct, nssc

let createSP (tb : ITextBuffer) spid (DocumentCoordinate ln) = 
    { id = spid
      document = defaultArg tb.FilePath (FilePath "")
      startLine = DocumentCoordinate ln
      startColumn = DocumentCoordinate 1
      endLine = DocumentCoordinate ln
      endColumn = DocumentCoordinate 100 }

let stubSp1 = createSP stubSpidXTb stubSpid1 stubSpidXLineNumber
let stubSp2 = createSP stubSpidXTb stubSpid2 stubSpidXLineNumber

let updateCoverageInfo (spCovData : list<SequencePointId * list<SimpleTestCase * option<SimpleTestResult>>>) 
    (ds : IDataStore) = 
    let ptir = PerTestIdResults()
    let pspiri = PerSequencePointIdTestRunId()
    
    let fldr spid () (tc : SimpleTestCase, tr : SimpleTestResult option) = 
        let b = ptir.GetOrAdd(tc.toTID(), fun _ -> ConcurrentBag<_>())
        tr |> Option.fold (fun () tr -> b.Add(tc.toTC() |> tr.toTR)) ()
        let b = pspiri.GetOrAdd(spid, fun _ -> ConcurrentBag<_>())
        
        let trid = 
            { testId = tc.toTID()
              testRunInstanceId = TestRunInstanceId(obj().GetHashCode()) }
        b.Add(trid)
    spCovData |> Seq.fold (fun () (spid, tctr) -> tctr |> Seq.fold (fldr spid) ()) ()
    (ptir, PerDocumentLocationTestFailureInfo(), pspiri)
    |> TestRunOutput
    |> ds.UpdateData

[<Fact>]
let ``Datastore SequencePointsUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createCCT "a.sln" stubSpidXTb (fun _ -> Seq.empty)
    PerDocumentSequencePoints()
    |> SequencePoints
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``Datastore TestResultsUpdated and CoverageInfoUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createCCT "a.sln" stubSpidXTb (fun _ -> Seq.empty)
    (PerTestIdResults(), PerDocumentLocationTestFailureInfo(), PerSequencePointIdTestRunId())
    |> TestRunOutput
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``If buffer doesnt have FileName return empty``() = 
    let tb = FakeTextBuffer("", null) :> ITextBuffer
    let _, tb, cct, _ = createCCT "a.sln" tb (fun _ -> Seq.empty)
    let ts = cct.GetTags(tb |> getNSSC (DocumentCoordinate 1))
    Assert.Empty(ts)

[<Fact>]
let ``If a line does not have any SequencePoints - return empty``() = 
    let _, tb, cct, _ = createCCT "a.sln" stubSpidXTb (fun _ -> Seq.empty)
    let ts = cct.GetTags(tb |> getNSSC stubSpidXLineNumber)
    Assert.Empty(ts)

[<Fact>]
let ``If SequencePoint has no test covering it - return empty``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid2, [] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 0), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)

[<Fact>]
let ``If SequencePoint has no tests covering it but DataStore does have TestId and TestResult - return empty``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid2, [ (stubTC1, Some stubTR1) ] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 0), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)
    Assert.Equal([| stubTC1.toTID() |], 
                 stubSpid2
                 |> ds.GetRunIdsForTestsCoveringSequencePointId
                 |> Seq.map (fun trid -> trid.testId))

[<Fact>]
let ``If SequencePoint has 1 TestId covering it but with 1 TestResults - return TestResult``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid1, [ (stubTC1, Some stubTR1) ] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 1), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| stubTR1 |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)
    Assert.Equal([| stubTC1 |], ts.[0].Tag.testResults |> Seq.map (fun tr -> tr.TestCase |> SimpleTestCase.fromTC))

[<Fact>]
let ``If SequencePoint has 1 TestId covering it but with 0 TestResults - return empty``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid1, [ (stubTC1, None) ] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 0), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)

[<Fact>]
let ``If SequencePoint has 1 TestId covering it but with 2 TestResults - return all TestResults``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid1, 
                               [ (stubTC1, Some stubTR1)
                                 (stubTC1, Some stubTR2) ] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 2), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| stubTR2; stubTR1 |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)
    Assert.Equal
        ([| stubTC1; stubTC1 |], ts.[0].Tag.testResults |> Seq.map (fun tr -> tr.TestCase |> SimpleTestCase.fromTC))

[<Fact>]
let ``If SequencePoint has 2 TestId covering it but with 1 TestResults each - return all TestResults``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1 ]
    ds |> updateCoverageInfo [ stubSpid1, 
                               [ (stubTC1, Some stubTR1)
                                 (stubTC2, Some stubTR2) ] ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal((1, 2), (ts.Length, ts.[0].Tag.testResults |> Seq.length))
    Assert.Equal([| stubTR2; stubTR1 |], ts.[0].Tag.testResults |> Seq.map SimpleTestResult.fromTR)
    Assert.Equal
        ([| stubTC2; stubTC1 |], ts.[0].Tag.testResults |> Seq.map (fun tr -> tr.TestCase |> SimpleTestCase.fromTC))

[<Fact>]
let ``If 2 SequencePoints have 1 TestId each covering it and with 1 TestResults each each with different TestOutcomes - return all of them``() = 
    let ds, cct, nssc = createCCT2 stubSpidXTb stubSpidXLineNumber [ stubSp1; stubSp2 ]
    ds |> updateCoverageInfo [ (stubSpid1, [ (stubTC1, Some stubTR1) ])
                               (stubSpid2, [ (stubTC2, Some stubTR2) ]) ]
    let ts = cct.GetTags(nssc) |> Seq.toArray
    Assert.Equal(2, ts.Length)
    Assert.Equal([| 1; 1 |], ts |> Seq.map (fun t -> t.Tag.testResults |> Seq.length))
    Assert.Equal([| stubTR1; stubTR2 |], 
                 ts
                 |> Seq.map (fun t -> t.Tag.testResults |> Seq.map SimpleTestResult.fromTR)
                 |> Seq.collect id)
    Assert.Equal([| stubTC1; stubTC2 |], 
                 ts
                 |> Seq.map (fun t -> t.Tag.testResults |> Seq.map (fun tr -> tr.TestCase |> SimpleTestCase.fromTC))
                 |> Seq.collect id)
