namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

module GlpyhBoundsGenerator = 
    open Microsoft.VisualStudio.Text.Formatting
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
    open System.Windows
    
    let generate ((topLeft, lines) : Point * seq<ITextViewLine>) = 
        let getGlyphBounds (r : Rect) = 
            let glyphSideLength = MarginConstants.Width * 0.8
            let glyphMargin = (MarginConstants.Width - glyphSideLength) / 2.0
            Rect
                (glyphMargin, (r.Top + r.Bottom) / 2.0 - glyphSideLength / 2.0 - topLeft.Y, glyphSideLength, 
                 glyphSideLength)
        lines |> Seq.map (fun l -> l.Bounds |> getGlyphBounds, l)
