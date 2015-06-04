namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows

type Margin(textView : IWpfTextView, tmta : ITagAggregator<_>, painter, getMarginSize, getVisualElement) = 
    let mutable disposed = false
    
    let throwIfDisposed() = 
        if disposed then raise (new ObjectDisposedException(MarginConstants.Name))
    
    let paintGlyphs() = 
        throwIfDisposed()
        (textView.ViewportLocation, textView.TextViewLines) ||> painter
    
    let textViewLayoutChanged _ _ = paintGlyphs()
    let lceh = new EventHandler<_>(textViewLayoutChanged)
    let testMarkerTagsChanged _ _ = paintGlyphs()
    let tmtceh = new EventHandler<_>(testMarkerTagsChanged)
    
    do 
        textView.LayoutChanged.AddHandler(lceh)
        tmta.TagsChanged.AddHandler(tmtceh)
    
    new(textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>) = 
        new Margin(textView, tmta, 
                   (new GlpyhPainter<FrameworkElement>(tmta.GetTags, GlyphFactory.create, MarginCanvas.Instance.Refresh)).Paint, 
                   (fun () -> MarginCanvas.Instance.ActualWidth), (fun () -> MarginCanvas.Instance :> FrameworkElement))
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
        member self.GetTextViewMargin(marginName : _) : _ = 
            throwIfDisposed()
            if marginName = MarginConstants.Name then self :> _
            else null
        
        // TT
        member __.MarginSize : _ = 
            throwIfDisposed()
            getMarginSize()
    
    // TT
    interface IWpfTextViewMargin with
        member __.VisualElement : _ = 
            throwIfDisposed()
            getVisualElement()
