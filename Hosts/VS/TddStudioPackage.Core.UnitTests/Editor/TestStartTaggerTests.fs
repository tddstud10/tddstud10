module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.TestStartTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging

let createTST s pdltp p t = 
    let ds = DataStore() :> IDataStore
    RunStartParams.Create (EngineConfig()) DateTime.Now (FilePath s) |> ds.UpdateRunStartParams
    let tb = FakeTextBuffer(t, p) :> ITextBuffer
    let tmt = TestStartTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    pdltp
    |> TestCases
    |> ds.UpdateData
    ds, tb, tmt, spy

let createPDLTP (ts : (string * FilePath * DocumentCoordinate) seq) = 
    let tpa = PerDocumentLocationDTestCases()
    
    let addTestCase (acc : PerDocumentLocationDTestCases) (f, d, l) = 
        let tc = { DtcId = Guid(); FullyQualifiedName = f; DisplayName = ""; Source = FilePath "src"; CodeFilePath = d; LineNumber = l }
        let b = 
            acc.GetOrAdd({ document = d
                           line = l }, fun _ -> ConcurrentBag<_>())
        b.Add(tc) |> ignore
        acc
    ts |> Seq.fold addTestCase tpa

let getNSSC n (tb : ITextBuffer) = 
    let ss = 
        tb.CurrentSnapshot.Lines
        |> Seq.skip (n - 1)
        |> Seq.take 1
        |> Seq.map (fun l -> l.Extent)
    NormalizedSnapshotSpanCollection(ss)

[<Fact>]
let ``Datastore TestCasesUpdated event fires TagsChanged event``() = 
    let _, tb, _, s = createTST @"c:\a.sln" (PerDocumentLocationDTestCases()) "" ""
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``GetTags returns empty if no tests are found in datastore``() = 
    let _, tb, tmt, _ = createTST @"sln.sln" (PerDocumentLocationDTestCases()) @"a.cs" """Line
Line 2
"""
    let ts = tmt.GetTags(tb |> getNSSC 1)
    Assert.Empty(ts)

[<Fact>]
let ``GetTags returns right tag for 1 empty and 1 each found and not found in datastore``() = 
    let pdltp = [ ("FQN:2nd line", FilePath @"a.cs", DocumentCoordinate 2) ] |> createPDLTP
    let _, tb, tmt, _ = createTST @"sln.sln" pdltp @"a.cs" """
Line 2
"""
    let ts = tmt.GetTags(tb |> getNSSC 1)
    Assert.Empty(ts)
    let ts = tmt.GetTags(tb |> getNSSC 2)
    Assert.Equal
        ([| ("FQN:2nd line", { document = FilePath "a.cs"; line = DocumentCoordinate 2 }) |], 
         ts |> Seq.collect (fun ts -> ts.Tag.TstTestCases |> Seq.map (fun t -> t.FullyQualifiedName, ts.Tag.TstLocation)))
