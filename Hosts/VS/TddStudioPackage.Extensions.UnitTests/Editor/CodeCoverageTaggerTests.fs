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
    RunExecutor.createRunStartParams DateTime.Now (FilePath s)
    |> ds.UpdateRunStartParams
    let tb = FakeTextBuffer(p, t) :> ITextBuffer
    let tmt = CodeCoverageTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    ds, tb, tmt, spy

[<Fact>]
let ``Datastore TestResultsUpdated and CoverageInfoUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createCCT @"c:\a.sln" "" ""
    (PerTestIdResults(), PerAssemblySequencePointsCoverage())
    |> TestRunOutput
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))
