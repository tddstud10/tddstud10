module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.GlyphGenerator

open System.Windows
open System.Windows.Input
open System
open System.Windows.Media
open System.Windows.Shapes
open System.ComponentModel.Design

let generate (showCM : Action<CommandID, int, int>) getZL ((b, gi) : Rect * MarginGlyphInfo) = 
    let showContextMenu t (e : Shape) (mbea : MouseButtonEventArgs) = 
        let menuID = gi.contextMenu
        let p = e.PointToScreen(mbea.GetPosition(e))
        ContextMenuData.Instance.GlyphTags <- t
        showCM.Invoke(menuID, int p.X, int p.Y)
    
    let e = 
        let p = Path()
        let gWidth = MarginConstants.Width * MarginConstants.GlyphWidthMarginWidthRatio * getZL()
        match gi.glyphType with
        | TestStart -> 
            p.Data <- Geometry.Parse
                          (String.Format("M {1} 0 L {0} {1} L {1} {0} M 0 {1} L {0} {1}", gWidth, gWidth / 2.0))
        | FailurePoint -> p.Data <- Geometry.Parse(String.Format("M 0 0 L {0} {0} M 0 {0} L {0} 0", gWidth))
        | CodeCoverage -> p.Data <- Geometry.Parse(String.Format("M 0 0 H {0} V {0} H 0 V 0", gWidth))
        p :> Shape
    
    e.Fill <- SolidColorBrush(gi.color)
    e.Stroke <- SolidColorBrush(gi.color)
    e.StrokeThickness <- 2.0 * getZL()
    e.MouseRightButtonUp.Add(showContextMenu gi.glyphTags e)
    b, e :> FrameworkElement
