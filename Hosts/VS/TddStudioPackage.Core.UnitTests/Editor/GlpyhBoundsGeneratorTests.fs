module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlpyhBoundGeneratorTests

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System
open System.Windows
open Xunit

let zl = 2.5
let getZL = fun () -> zl

let getMTSForline (ss : SnapshotSpan) : IMappingTagSpan<_> seq = 
    let mts = FakeMappingTagSpan<TestStartTag>()
    
    let f () p = 
        mts.Tag <- { TstTestCases = 
                         [ { DtcId = Guid()
                             FullyQualifiedName = "FQN:" + ss.GetText()
                             DisplayName = ""
                             Source = FilePath "source"
                             CodeFilePath = FilePath ""
                             LineNumber = DocumentCoordinate 0 } ]
                     TstLocation = 
                         { document = p
                           line = DocumentCoordinate(ss.Start.GetContainingLine().LineNumber + 1) } }
    ss.Snapshot.TextBuffer.FilePath |> Option.fold f ()
    upcast [ mts :> IMappingTagSpan<_> ]

[<Fact>]
let ``Empty enumeration returned if there are no input lines``() = 
    let es = (Point(0.0, 0.0), [] :> ITextViewLine seq) |> GlpyhBoundsGenerator.generate getZL
    Assert.Empty(es)

[<Fact>]
let ``Check glyph positions returned when there is 1 empty line and 2 non empty ones``() = 
    let content = """first non-empty line

third non-empty line"""
    let tv = FakeWpfTextView(Point(100.0, 50.0), 25.0, zl, content)
    let lines = (tv :> ITextView).TextViewLines :> ITextViewLine seq
    let es = ((tv :> IWpfTextView).ViewportLocation, lines) |> GlpyhBoundsGenerator.generate getZL
    Assert.Equal([| Rect(2.5, 21.25, 20.0, 20.0)
                    Rect(2.5, 83.75, 20.0, 20.0)
                    Rect(2.5, 146.25, 20.0, 20.0) |], es |> Seq.map fst)
