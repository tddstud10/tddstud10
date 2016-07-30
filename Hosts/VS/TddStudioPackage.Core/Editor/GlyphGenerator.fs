module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
open System
open System.Windows
open System.Windows.Media

let generate createHA getZL ((b, mgi) : Rect * MarginGlyphInfo) = 
    let gi = 
        let shape = 
            let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
            match mgi.Type with
            | TestStart -> 
                Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
            | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
            | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
        GlyphInfo(Shape = shape, Color = mgi.Color, OutlineThickness = 2.0 * getZL())
    
    let e = MainUserControl(DataContext = MainViewModel(gi, createHA mgi.Tags))
    b, e :> FrameworkElement
