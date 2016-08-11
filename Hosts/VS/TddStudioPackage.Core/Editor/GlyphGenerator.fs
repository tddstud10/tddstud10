module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
open System
open System.Windows
open System.Windows.Media
open System.Windows.Shapes

module XX =
    open System.Collections.Generic
    open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
    open Microsoft.VisualStudio.Text
    open Microsoft.VisualStudio.Text.Formatting
    open Microsoft.VisualStudio.Text.Tagging

    let mutable bypass = false
    let mutable useCache = false
    let mutable canvasV2 = true;
    let stopwatch = Diagnostics.Stopwatch()
    let xxx (cache : IDictionary<SnapshotSpan, IEnumerable<IMappingTagSpan<IMarginGlyphTag>>>) (l : ITextViewLine) (mgta : ITagAggregator<_>) =
        stopwatch.Restart()
        let found, ret = 
            if bypass then
                false, Seq.empty
            else
                if useCache then
                    let found, value = cache.TryGetValue l.Extent
                    let ret = 
                        if found then
                            value
                        else
                            let ret = l.Extent |> mgta.GetTags |> Seq.toArray
                            cache.Add(l.Extent, ret)
                            ret :> seq<_>
                    found, ret
                else
                    false, l.Extent |> mgta.GetTags
        Logger.logErrorf "|>>>> mgta.GetTags called: Took %d ms for %d tags. In cache? %O." stopwatch.Elapsed.Milliseconds (Seq.length ret) found
        ret    

let generate createHA getZL ((b, mgi) : Rect * MarginGlyphInfo) = 
    let shape = 
        let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
        match mgi.Type with
        | TestStart -> 
            Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
        | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
        | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
    if XX.canvasV2 then
        let br = SolidColorBrush(mgi.Color)
        let e = Path(Data = shape, Fill = br, Stroke = br)
        e.Tag <- createHA mgi.Tags
        //e.MouseDown.AddHandler(xxx)
        b, e :> FrameworkElement
    else
        let gi = 
            GlyphInfo(Shape = shape, Color = mgi.Color, OutlineThickness = 2.0 * getZL())
    
        let e = MainUserControl(DataContext = MainViewModel(gi, createHA mgi.Tags))
        b, e :> FrameworkElement
