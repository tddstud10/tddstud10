namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows

module GlyphFactory =
    open System.Windows.Shapes
    open System.Windows.Media

    let createGlyphForTag ((t, r) : TestMarkerTag * Rect) =
        let e = new Ellipse()
        e.Stroke <- new SolidColorBrush(Colors.Green)
        e.StrokeThickness <- 1.5
        e.Tag <- t
        e :> FrameworkElement, r
