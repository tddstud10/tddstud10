namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows
open Microsoft.VisualStudio.Text.Formatting

type Margin(textView : IWpfTextView, tmta : ITagAggregator<_>, painter, getMarginSize, getVisualElement) = 
    let mutable disposed = false
    
    let throwIfDisposed() = 
        if disposed then raise (new ObjectDisposedException(MarginConstants.Name))
    
    let paintGlyphs() = 
        throwIfDisposed()
        (textView.ViewportLocation, textView.TextViewLines :> ITextViewLine seq) |> painter
    
    let lcSub = textView.LayoutChanged.Subscribe(fun _ -> paintGlyphs())
    let tcSub = tmta.TagsChanged.Subscribe(fun _ -> paintGlyphs())

    new(textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>) = 
        let canvas = MarginCanvas()
        new Margin(textView, tmta, 
                   (new MarginGlpyhTagAndBoundGenerator(tmta.GetTags)).Generate >> Seq.map GlyphFactory.createGlyphForTag >> canvas.Refresh,
                   (fun () -> canvas.ActualWidth), (fun () -> canvas :> FrameworkElement))
    override x.Finalize() = x.Dispose(false)
    
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                lcSub.Dispose()
                tcSub.Dispose()
            disposed <- true
    
    interface IDisposable with
        member x.Dispose() : _ = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
    
    interface ITextViewMargin with
        
        member __.Enabled : _ = 
            throwIfDisposed()
            true
        
        member self.GetTextViewMargin(marginName : _) : _ = 
            throwIfDisposed()
            if marginName = MarginConstants.Name then upcast self
            else null
        
        member __.MarginSize : _ = 
            throwIfDisposed()
            getMarginSize()
    
    interface IWpfTextViewMargin with
        member __.VisualElement : _ = 
            throwIfDisposed()
            getVisualElement()
