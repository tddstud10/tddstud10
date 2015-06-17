namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows
open System.Windows.Input
open System
open System.Windows.Shapes
open System.Windows.Media
open System.ComponentModel.Design
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions

module GlyphFactory = 
    let createGlyphForTag showCM ((t, r) : TestStartTag * Rect) = 
        let showContextMenu (showCM : Action<CommandID, int, int>) t (e : Ellipse) (mbea : MouseButtonEventArgs) = 
            let menuID = new CommandID(Guid(PkgGuids.GuidGlyphContextCmdSet), PkgCmdID.GlyphContextMenu |> int)
            let p = e.PointToScreen(mbea.GetPosition(e))
            ContextMenuData.Instance.TestCase <- Some t
            showCM.Invoke(menuID, int p.X, int p.Y)
        
        let e = new Ellipse()
        e.Stroke <- new SolidColorBrush(Colors.Green)
        e.StrokeThickness <- 4.0
        e.MouseRightButtonUp.Add(fun mbea -> showContextMenu showCM t.testCase e mbea)
        e :> FrameworkElement, r
