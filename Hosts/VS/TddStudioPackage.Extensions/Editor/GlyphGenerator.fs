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
    
    let e = 
        let p = Path()
        match gi.glyphType with
        | TestStart ->
            p.Data <- Geometry.Parse("M 4 0 L 8 4 L 4 8 M 0 4 L 8 4")
        | FailurePoint -> 
            p.Data <- Geometry.Parse("M 0 0 L 8 8 M 0 8 L 8 0")
        | CodeCoverage -> 
            p.Data <- Geometry.Parse("M 0 0 H 8 V 8 H 0 V 0")
        p :> Shape

    e.Fill <- SolidColorBrush(gi.color)
    e.Stroke <- SolidColorBrush(gi.color)
    e.StrokeThickness <- 2.0
    e.MouseRightButtonUp.Add(showContextMenu gi.glyphTags e)
    b, e :> FrameworkElement
