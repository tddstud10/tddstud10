namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

module GlpyhBoundsGenerator = 
    open Microsoft.VisualStudio.Text.Formatting
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
    open System.Windows
    
    let generate getZL ((topLeft, lines) : Point * seq<ITextViewLine>) = 
        let getGlyphBounds (r : Rect) = 
            let glyphSideLength = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
            let glyphMargin = (MarginConstants.Width * getZL() - glyphSideLength) / 2.0
            Rect
                (glyphMargin, (r.Top + r.Bottom) / 2.0 * getZL() - glyphSideLength / 2.0 - topLeft.Y * getZL(), 
                 glyphSideLength, glyphSideLength)
        lines |> Seq.map (fun l -> l.Bounds |> getGlyphBounds, l)
