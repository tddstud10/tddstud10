namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows
open System.Windows.Input
open System
open Microsoft.VisualStudio.TestPlatform.ObjectModel

type ContextMenuData() =
    static let instance = lazy ContextMenuData()
    // Change to none
    member val TestCase : TestCase = null with get, set

    static member Instance 
        with public get () = instance.Value

module GlyphFactory =
    open System.Windows.Shapes
    open System.Windows.Media
    open System.ComponentModel.Design

    let xxxxx (menuSvc : IMenuCommandService) t (e : Ellipse) (mbea : MouseButtonEventArgs) =
        let menuID = new CommandID(Guid("{1E198C22-5980-4E7E-92F3-F73168D1FB63}"), 0x5000)
        let p = e.PointToScreen(mbea.GetPosition(e));
        ContextMenuData.Instance.TestCase <- t
        menuSvc.ShowContextMenu(menuID, int p.X, int p.Y);

    let createGlyphForTag (menuSvc : IMenuCommandService) ((t, r) : TestMarkerTag * Rect) =

        let e = new Ellipse()
        e.Stroke <- new SolidColorBrush(Colors.Green)
        e.StrokeThickness <- 4.0
        // Do using command bindings
        e.MouseRightButtonUp.Add(fun mbea -> xxxxx menuSvc t.testCase e mbea)
        e :> FrameworkElement, r
