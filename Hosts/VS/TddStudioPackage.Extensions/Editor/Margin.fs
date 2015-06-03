namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System.Windows
open System.Windows.Shapes
open System
open System.Windows.Media
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

type Margin(textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>) = 
    let mutable disposed = false
    let canvas = new MarginCanvas()
    
    // Move to Glyph Factory
    let createGlyph t = 
        let ellipse = new Ellipse()
        ellipse.Stroke <- new SolidColorBrush(Colors.Green)
        ellipse.StrokeThickness <- 1.5
        ellipse.Tag <- t
        ellipse :> FrameworkElement
    
    let getBoundsAndTags (lines : ITextViewLine seq) = 
        lines
        |> Seq.map (fun l -> l, l.Extent)
        |> Seq.filter (fun (_, ss) -> not ss.IsEmpty)
        |> Seq.collect (fun (l, ss) -> tmta.GetTags(ss) |> Seq.map (fun t -> l, t))
        |> Seq.map (fun (l, mts) -> l.Bounds, mts.Tag)
    
    let refreshMargin() = 
        textView.TextViewLines
        |> getBoundsAndTags
        |> Seq.map (fun (b, t) -> b, t |> createGlyph)
        |> canvas.Refresh (textView.ViewportLocation)
    
    let textViewLayoutChanged _ _ = refreshMargin()
    let lceh = new EventHandler<_>(textViewLayoutChanged)
    let testMarkerTagsChanged _ _ = canvas.Dispatcher.Invoke(refreshMargin)
    let tmtceh = new EventHandler<_>(testMarkerTagsChanged)
    
    do 
        textView.LayoutChanged.AddHandler(lceh)
        tmta.TagsChanged.AddHandler(tmtceh)
    
    let throwIfDisposed() = 
        if disposed then raise (new ObjectDisposedException(MarginConstants.Name))
    
    override x.Finalize() = x.Dispose(false)
    
    // TT
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                tmta.TagsChanged.RemoveHandler(tmtceh)
                textView.LayoutChanged.RemoveHandler(lceh)
            disposed <- true
    
    interface IDisposable with
        member x.Dispose() : _ = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
    
    interface ITextViewMargin with
        
        // TT
        member x.Enabled : _ = 
            throwIfDisposed()
            true
        
        // TT
        member x.GetTextViewMargin(marginName : _) : _ = 
            if marginName = MarginConstants.Name then x :> _
            else null
        
        // TT
        member x.MarginSize : _ = 
            throwIfDisposed()
            canvas.ActualWidth
    
    // TT
    interface IWpfTextViewMargin with
        member x.VisualElement : _ = 
            throwIfDisposed()
            canvas :> _
