module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.SequencePointTaggerTests

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open System.Collections.Concurrent

let createSPT s pdsp p t = 
    let ds = DataStore() :> IDataStore
    let tb = FakeTextBuffer(t, p) :> ITextBuffer
    let spt = SequencePointTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    spt.TagsChanged.Add(spy.Func >> ignore)
    RunExecutor.createRunStartParams DateTime.Now (FilePath s) |> ds.UpdateRunStartParams
    pdsp
    |> SequencePoints
    |> ds.UpdateData
    ds, tb, spt, spy

let createPDSP p (sps : (int * int * int * int) seq) = 
    let pdsp = PerDocumentSequencePoints()
    
    let addTestCase (acc : PerDocumentSequencePoints) (sl, sc, el, ec) = 
        let b = acc.GetOrAdd(FilePath p, fun _ -> ConcurrentBag<_>())
        { id = 
              { methodId = 
                    { assemblyId = Guid.NewGuid() |> AssemblyId
                      mdTokenRid = 0xbadf00du |> MdTokenRid }
                uid = 0xbadf00d }
          document = FilePath p
          startLine = sl |> DocumentCoordinate
          startColumn = sc |> DocumentCoordinate
          endLine = el |> DocumentCoordinate
          endColumn = ec |> DocumentCoordinate }
        |> b.Add
        |> ignore
        acc
    sps |> Seq.fold addTestCase pdsp

let getNSSC n (tb : ITextBuffer) = 
    let ss = 
        tb.CurrentSnapshot.Lines
        |> Seq.skip (n - 1)
        |> Seq.take 1
        |> Seq.map (fun l -> l.Extent)
    NormalizedSnapshotSpanCollection(ss)

let getTSBounds1Based (t : ITagSpan<_>) = 
    let s, e = t.Span.Start, t.Span.End
    s.GetContainingLine().LineNumber + 1, s.Difference(s) + 1, e.GetContainingLine().LineNumber + 1, 
    s.Difference(e) + 1 - 1

[<Fact>]
let ``Datastore SequencePointsUpdated event fires TagsChanged event``() = 
    let _, tb, _, s = createSPT "a.sln" (PerDocumentSequencePoints()) null ""
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``If buffer doesnt have FileName return empty``() = 
    let _, tb, spt, _ = createSPT "sln.sln" (PerDocumentSequencePoints()) null ""
    let ts = spt.GetTags(tb |> getNSSC 1)
    Assert.Empty(ts)

[<Fact>]
let ``If span doesnt have any sequence points return empty``() = 
    let t = """
........
"""
    let _, tb, spt, _ = createSPT "sln.sln" (PerDocumentSequencePoints()) "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 1)
    Assert.Empty(ts)

[<Fact>]
let ``If span corresponds to a full sequence point return that``() = 
    let t = """
++++++
........
"""
    let pdsp = createPDSP "file.cpp" [ (2, 1, 2, 6) ]
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 2)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (2, 1, 2, 6) |], ts |> Seq.map getTSBounds1Based)

[<Fact>]
let ``If span lies within a sequence point return that``() = 
    let t = """
...++
++++++
++......
"""
    let pdsp = createPDSP "file.cpp" [ (2, 4, 4, 2) ]
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 3)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (3, 1, 3, 6) |], ts |> Seq.map getTSBounds1Based)

[<Fact>]
let ``If span intersects with a start of a sequence point return that``() = 
    let t = """
....++++
"""
    let pdsp = createPDSP "file.cpp" [ (2, 5, 2, 8) ]
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 2)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (2, 1, 2, 8) |], ts |> Seq.map getTSBounds1Based)

[<Fact>]
let ``If span intersects with a end of a sequence point return that``() = 
    let t = """
++++..
"""
    let pdsp = createPDSP "file.cpp" [ (2, 1, 2, 6) ]
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 2)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (2, 1, 2, 6) |], ts |> Seq.map getTSBounds1Based)

[<Fact>]
let ``If span completely covers a sequence point return that``() = 
    let t = """
.++++..
"""
    let pdsp = createPDSP "file.cpp" [ (2, 2, 2, 7) ]
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 2)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (2, 1, 2, 7) |], ts |> Seq.map getTSBounds1Based)

[<Fact>]
let ``If span covers leading end of SP1, full SP2, trailing start of SP3, return all 3``() = 
    let t = """
..++
++..++...++
+++..
"""
    
    let pdsp = 
        createPDSP "file.cpp" [ (2, 3, 3, 2)
                                (3, 5, 3, 6)
                                (3, 10, 4, 3) ]
    
    let _, tb, spt, _ = createSPT "sln.sln" pdsp "file.cpp" t
    let ts = spt.GetTags(tb |> getNSSC 3)
    Assert.Equal(pdsp.[FilePath "file.cpp"].ToArray(), ts |> Seq.map (fun t -> t.Tag.spx))
    Assert.Equal([| (3, 1, 3, 11)
                    (3, 1, 3, 11)
                    (3, 1, 3, 11) |], ts |> Seq.map getTSBounds1Based)
// getFilePath to property
// DRY in trygetvalue in datastore
// getTSBounds1Based DRY violation with product code
// spx
(*
Assume:
- TagSpan.Span list the whole line, not just the sequence pointer intersection
- Span passed to GetTags is on a single line only

Thinking:
- T: Throw if there is more than one span and it is multiline
v T: If document doesn thave file name, return empty
- Filter the tagger by the buffer filename
- Given a span, it can correspond to:
  v T: No SP
  v T: Full SP
  v T: Wholly lies within an SP
  v T: Leading SP
  v T: Trailing SP
  v T: SP lines wholly withing snapshotspan
  v T: {0, 1} Partial SP + {0..} complete SP + {0, 1} Partial SP
v T: Multiple sequence points
v T: Multiple tags returned
- IL is 1,1 based, Editor is 0,0 based

GetTags:
- Filter seq<seqPt(sl, sc, el, ec)>: seqPt.document = textBuffer.file
- span(s, len) -> seq<seqPt(sl, sc, el, ec)>
  - Filter seq<seqPt(sl, sc, el, ec)>
  - span(s, len) -> (sl = l + 1, sc = s + 1, el = l + 1 , ec = s + len + 1)
 *)
