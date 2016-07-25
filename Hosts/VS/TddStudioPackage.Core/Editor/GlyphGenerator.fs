module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphGenerator

open System.Windows
open System.Windows.Input
open System
open System.Windows.Media
open System.Windows.Shapes
open System.ComponentModel.Design
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel

let generate (showCM : Action<CommandID, int, int>) getZL ((b, gi) : Rect * MarginGlyphInfo) = 
    let showContextMenu t (e : Shape) (mbea : MouseButtonEventArgs) = 
        let menuID = gi.contextMenu
        let p = e.PointToScreen(mbea.GetPosition(e))
        ContextMenuData.Instance.GlyphTags <- t
        showCM.Invoke(menuID, int p.X, int p.Y)
    
    let e = MainUserControl()
    let geo =
        let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
        match gi.glyphType with
        | TestStart -> Geometry.Parse(String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
        | FailurePoint -> Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
        | CodeCoverage -> Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))

    let ctrs = 
        gi.glyphTags 
        |> Seq.filter (fun it -> it :? CodeCoverageTag)
        |> Seq.map (fun it -> it :?> CodeCoverageTag)
        |> Seq.collect (fun it -> it.CCTTestResults)
    e.DataContext <- MainViewModel(geo, gi.color, 2.0 * getZL(), ctrs)
    b, e :> FrameworkElement
