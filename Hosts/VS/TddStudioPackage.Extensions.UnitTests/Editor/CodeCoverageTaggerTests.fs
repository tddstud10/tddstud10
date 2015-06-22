module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.CodeCoverageTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging

let createCCT s p t = 
    let ds = DataStore() :> IDataStore
    RunStartParamsExtensions.create DateTime.Now (FilePath s)
    |> ds.UpdateRunStartParams
    let tb = FakeTextBuffer(t, p) :> ITextBuffer
    let ta = new FakeTagAggregator<_>()
    let tmt = CodeCoverageTagger(tb, ta, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    ds, tb, tmt, spy

let getNSSC n (tb : ITextBuffer) = 
    let ss = 
        tb.CurrentSnapshot.Lines
        |> Seq.skip (n - 1)
        |> Seq.take 1
        |> Seq.map (fun l -> l.Extent)
    NormalizedSnapshotSpanCollection(ss)

[<Fact>]
let ``Datastore SequencePointsUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createCCT @"c:\a.sln" "" ""
    PerDocumentSequencePoints()
    |> SequencePoints
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``Datastore TestResultsUpdated and CoverageInfoUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createCCT @"c:\a.sln" "" ""
    (PerTestIdResults(), PerDocumentLocationTestFailureInfo(), PerSequencePointIdTestRunId())
    |> TestRunOutput
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``If buffer doesnt have FileName return empty``() = 
    let _, tb, spt, _ = createCCT @"c:\a.sln" "" ""
    let ts = spt.GetTags(tb |> getNSSC 1)
    Assert.Empty(ts)
