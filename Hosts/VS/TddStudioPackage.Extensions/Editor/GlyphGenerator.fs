module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.GlyphGenerator

open System.Windows
open System.Windows.Input
open System
open System.Windows.Media
open System.Windows.Shapes
open System.ComponentModel.Design

let generate (showCM : Action<CommandID, int, int>) ((b, gi) : Rect * MarginGlyphInfo) = 
    let showContextMenu t (e : Shape) (mbea : MouseButtonEventArgs) = 
        let menuID = gi.contextMenu
        let p = e.PointToScreen(mbea.GetPosition(e))
        ContextMenuData.Instance.GlyphTags <- t
        showCM.Invoke(menuID, int p.X, int p.Y)
    
    let e : Shape = 
        match gi.glyphType with
        | TestStart -> upcast Ellipse()
        | FailurePoint -> 
            let p = Path()
            p.Data <- Geometry.Parse("M 0 0 L 8 8 L 0 10 M 0 8 L 8 0")
            upcast p
        | CodeCoverageFull -> upcast Rectangle()
        | CodeCoveragePartial -> 
            let p = Path()
            p.Data <- Geometry.Parse("M 0 0 L 10 0 L 0 10 Z")
            upcast p

    e.Stroke <- SolidColorBrush(gi.color)
    e.StrokeThickness <- 3.0
    e.MouseRightButtonUp.Add(showContextMenu gi.glyphTags e)
    b, e :> FrameworkElement
