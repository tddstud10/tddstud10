namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

module GlyphGenerator = 
    open System.Windows
    open System.Windows.Input
    open System
    open System.Windows.Media
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions
    open System.Windows.Shapes
    open System.ComponentModel.Design
    
    let generate (showCM : Action<CommandID, int, int>) ((b, gi) : Rect * MarginGlyphInfo) = 
        let showContextMenu t (e : Ellipse) (mbea : MouseButtonEventArgs) = 
            let menuID = CommandID(Guid(PkgGuids.GuidGlyphContextCmdSet), PkgCmdID.GlyphContextMenu |> int)
            let p = e.PointToScreen(mbea.GetPosition(e))
            ContextMenuData.Instance.GlyphTag <- Some t
            showCM.Invoke(menuID, int p.X, int p.Y)
        
        let e = Ellipse()
        e.Stroke <- SolidColorBrush(Colors.Green)
        e.StrokeThickness <- 4.0
        e.MouseRightButtonUp.Add(showContextMenu gi.glyphTag e)
        b, e :> FrameworkElement
