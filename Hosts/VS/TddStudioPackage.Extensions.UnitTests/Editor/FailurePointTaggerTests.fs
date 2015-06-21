module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.FailurePointTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

let createFPT s pdltfi p t = 
    let ds = DataStore() :> IDataStore
    let tb = FakeTextBuffer(t, p) :> ITextBuffer
    let tmt = FailurePointTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    RunStartParamsExtensions.create DateTime.Now (FilePath s) |> ds.UpdateRunStartParams
    (PerTestIdResults(), pdltfi, PerAssemblySequencePointsCoverage())
    |> TestRunOutput
    |> ds.UpdateData
    ds, tb, tmt, spy

let createPDLTFI() = 
    let dl1 = 
        { document = FilePath @"f3.cpp"
          line = DocumentCoordinate 1 }
    
    let dl2 = 
        { document = FilePath @"f5.cpp"
          line = DocumentCoordinate 1 }

    let tfi = 
        { message = ""
          stack = 
              [| ParsedFrame("NS.C.M(T p)", dl1)
                 UnparsedFrame("at XNS.XC.XM()")
                 ParsedFrame("xNS.xC.xM(T p0, T p1)", dl2) |] }
    
    let pdltfi = PerDocumentLocationTestFailureInfo()
    let b = ConcurrentBag<TestFailureInfo>()
    b.Add(tfi)
    b.Add(tfi)
    pdltfi.TryAdd(dl1, b) |> ignore
    pdltfi.TryAdd(dl2, b) |> ignore
    pdltfi

let getNSSC n (tb : ITextBuffer) = 
    let ss = 
        tb.CurrentSnapshot.Lines
        |> Seq.skip (n - 1)
        |> Seq.take 1
        |> Seq.map (fun l -> l.Extent)
    NormalizedSnapshotSpanCollection(ss)

[<Fact>]
let ``Datastore TestResultsUpdated and CoverageInfoUpdated event fires TagsChanged event``() = 
    let _, tb, _, s = createFPT @"c:\a.sln" (PerDocumentLocationTestFailureInfo()) "" ""
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``If buffer doesnt have FileName return empty``() = 
    let _, tb, fpt, _ = createFPT @"c:\a.sln" (PerDocumentLocationTestFailureInfo()) "" ""
    let it = fpt.GetTags(tb |> getNSSC 1)
    Assert.Empty(it)

[<Fact>]
let ``If no TestFailureInfo exists for current line, return empty``() = 
    let t = """
"""
    let _, tb, fpt, _ = createFPT "sln.sln" (PerDocumentLocationTestFailureInfo()) "file.cpp" t
    let it = fpt.GetTags(tb |> getNSSC 1)
    Assert.Empty(it)

[<Fact>]
let ``If TestFailureInfo exists for current line, return those``() = 
    let pdltfi = createPDLTFI()
    let t = """line 1
"""
    let _, tb, fpt, _ = createFPT "sln.sln" pdltfi "f3.cpp" t
    let it = fpt.GetTags(tb |> getNSSC 1)
    Assert.Equal(pdltfi.[{document = "f3.cpp" |> FilePath; line = 1 |> DocumentCoordinate }]
                 |> Seq.toArray, 
                 it
                 |> Seq.map (fun t -> t.Tag.tfis)
                 |> Seq.collect id)
    Assert.Equal([| (1, 1, 1, 6) |], it |> Seq.map (fun t -> t.Span.Bounds1Based))
