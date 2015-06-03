namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows

type GlpyhPainter() =
    do ()

type Margin(textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>, glyphCreator : TestMarkerTag -> FrameworkElement) =
    let mutable disposed = false
    let canvas = new MarginCanvas()
    
    let getBoundsAndTags (lines : ITextViewLine seq) = 
        lines
        |> Seq.map (fun l -> l, l.Extent)
        |> Seq.filter (fun (_, ss) -> not ss.IsEmpty)
        |> Seq.collect (fun (l, ss) -> tmta.GetTags(ss) |> Seq.map (fun t -> l, t))
        |> Seq.map (fun (l, mts) -> l.Bounds, mts.Tag)
    
    let refreshMargin() = 
        textView.TextViewLines
        |> getBoundsAndTags
        |> Seq.map (fun (b, t) -> b, t |> glyphCreator)
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

    new (textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>) = new Margin (textView, tmta, GlyphFactory.create) 
    
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
        member __.Enabled : _ = 
            throwIfDisposed()
            true
        
        // TT
        member x.GetTextViewMargin(marginName : _) : _ = 
            if marginName = MarginConstants.Name then x :> _
            else null
        
        // TT
        member __.MarginSize : _ = 
            throwIfDisposed()
            canvas.ActualWidth
    
    // TT
    interface IWpfTextViewMargin with
        member __.VisualElement : _ = 
            throwIfDisposed()
            canvas :> _

#if DONT_COMPILE
- ctor: 

#endif