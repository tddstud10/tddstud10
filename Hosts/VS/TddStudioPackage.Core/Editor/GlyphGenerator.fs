module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
open System
open System.Windows
open System.Windows.Media

let generate (ha : HostIdeApi) getZL ((b, mgi) : Rect * MarginGlyphInfo) = 
    let ha = HostIdeActions(GotoTest = Action<_>(ha.GotoTest), DebugTest = Action<_>(ha.RunTest), RunTest = Action<_>(ha.DebugTest))

    let gi = 
        let shape = 
            let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
            match mgi.glyphType with
            | TestStart -> 
                Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
            | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
            | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
        GlyphInfo(Shape = shape, Color = mgi.color, OutlineThickness = 2.0 * getZL())

    let ctrs = 
        mgi.glyphTags
        |> Seq.filter (fun it -> it :? CodeCoverageTag)
        |> Seq.map (fun it -> it :?> CodeCoverageTag)
        |> Seq.collect (fun it -> it.CCTTestResults)
    
    let e = MainUserControl()
    e.DataContext <- MainViewModel(ha, gi, ctrs)
    b, e :> FrameworkElement
