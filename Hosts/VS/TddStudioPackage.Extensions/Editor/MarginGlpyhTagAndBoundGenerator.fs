namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows
open Microsoft.VisualStudio.Text

type MarginGlpyhTagAndBoundGenerator(tagsGetter : SnapshotSpan -> IMappingTagSpan<TestMarkerTag> seq) = 
    member __.Generate((topLeft, lines) : Point * ITextViewLine seq) = 
        let getGlyphBounds (r : Rect) =
            let glyphSideLength = MarginConstants.Width * 0.8
            let glyphMargin =  (MarginConstants.Width - glyphSideLength) / 2.0
            new Rect(r.X - topLeft.X + glyphMargin, (r.Top + r.Bottom) / 2.0 - glyphSideLength / 2.0 - topLeft.Y, glyphSideLength, glyphSideLength)
        lines
        |> Seq.map (fun l -> l.Bounds |> getGlyphBounds, l.Extent)
        |> Seq.filter (fun (_, ss) -> not ss.IsEmpty)
        |> Seq.collect (fun (b, ss) -> 
               ss
               |> tagsGetter
               |> Seq.map (fun t -> t.Tag, b))
