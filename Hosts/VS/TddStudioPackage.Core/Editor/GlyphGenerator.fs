module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
open System
open System.Windows
open System.Windows.Media

let generate (ha : HostIdeApi) getZL ((b, mgi) : Rect * MarginGlyphInfo) = 
    let ha = HostIdeActions(GotoTest = ha.GotoTest, DebugTest = ha.DebugTest, RunTest = ha.RunTest, IdeInDebugMode = ha.IdeInDebugMode)

    let gi = 
        let shape = 
            let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
            match mgi.Type with
            | TestStart -> 
                Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
            | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
            | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
        GlyphInfo(Shape = shape, Color = mgi.Color, OutlineThickness = 2.0 * getZL())

    let ctrs = 
        mgi.Tags
        |> Seq.filter (fun it -> it :? CodeCoverageTag)
        |> Seq.map (fun it -> 
                    let cct = it :?> CodeCoverageTag
                    cct.CctSeqPoint, cct.CctTestResults)
   
    let e = MainUserControl()
    e.DataContext <- MainViewModel(ha, gi, ctrs)
    b, e :> FrameworkElement
