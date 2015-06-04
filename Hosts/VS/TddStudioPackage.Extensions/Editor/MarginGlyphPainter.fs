namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows
open Microsoft.VisualStudio.Text

type MarginGlyphEntry<'T> = Rect * 'T

type GlpyhPainter<'T>(tagsGetter : SnapshotSpan -> IMappingTagSpan<TestMarkerTag> seq, glyphCreator : TestMarkerTag -> 'T, refreshCanvas : Point -> MarginGlyphEntry<'T> seq -> unit) = 
    member __.Paint (topLeft : Point) (lines : ITextViewLine seq) = 
        lines
        |> Seq.map (fun l -> l.Bounds, l.Extent)
        |> Seq.filter (fun (_, ss) -> not ss.IsEmpty)
        |> Seq.collect (fun (b, ss) -> 
                ss
                |> tagsGetter
                |> Seq.map (fun t -> b, t.Tag))
        |> Seq.map (fun (b, t) -> b, t |> glyphCreator)
        |> refreshCanvas topLeft

#if DONT_COMPILE
#endif
