namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows

module GlyphFactory =
    open System.Windows.Shapes
    open System.Windows.Media

    let create t =
        let ellipse = new Ellipse()
        ellipse.Stroke <- new SolidColorBrush(Colors.Green)
        ellipse.StrokeThickness <- 1.5
        ellipse.Tag <- t
        ellipse :> FrameworkElement
