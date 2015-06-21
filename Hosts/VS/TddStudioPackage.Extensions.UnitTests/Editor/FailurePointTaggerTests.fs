module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.FailurePointTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging

let createFPT s p t = 
    let ds = DataStore() :> IDataStore
    RunStartParamsExtensions.create DateTime.Now (FilePath s)
    |> ds.UpdateRunStartParams
    let tb = FakeTextBuffer(t, p) :> ITextBuffer
    let tmt = FailurePointTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    ds, tb, tmt, spy

[<Fact>]
let ``Datastore TestResultsUpdated and CoverageInfoUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createFPT @"c:\a.sln" "" ""
    (PerTestIdResults(), PerDocumentLocationTestFailureInfo(), PerAssemblySequencePointsCoverage())
    |> TestRunOutput
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))
