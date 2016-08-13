module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open System
open System.Windows
open System.Windows.Media
open System.Windows.Shapes

let generate createHA getZL ((b, mgi) : Rect * MarginGlyphInfo) = 
    let shape = 
        let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
        match mgi.Type with
        | TestStart -> 
            Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
        | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
        | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
    
    let br = SolidColorBrush(mgi.Color)
    let e = Path(Data = shape, Fill = br, Stroke = br, StrokeThickness = 2.0 * getZL())
    b, e :> FrameworkElement, createHA mgi.Tags
