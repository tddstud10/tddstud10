namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows
open Microsoft.VisualStudio.Text.Formatting

type Margin(textView : IWpfTextView, mgta : ITagAggregator<_>, __, painter, getMarginSize, getVisualElement) = 
    let mutable disposed = false
    
    let throwIfDisposed() = 
        if disposed then raise (ObjectDisposedException(MarginConstants.Name))
    
    let paintGlyphs() = 
        throwIfDisposed()
        (textView.ViewportLocation, textView.TextViewLines :> _ seq) |> painter
    
    let lcSub = textView.LayoutChanged.Subscribe(fun _ -> paintGlyphs())
    let tcSub = mgta.TagsChanged.Subscribe(fun _ -> paintGlyphs())

    new(textView : IWpfTextView, mgta : ITagAggregator<_>, showCM) = 
        let canvas = MarginCanvas()
        let f1 = GlpyhBoundsGenerator.generate
        let f2 = Seq.map (fun (b, (l : ITextViewLine)) -> b, l.Extent |> mgta.GetTags)
        let f3 = Seq.choose GlyphInfoGenerator.generate
        let f4 = Seq.map (GlyphGenerator.generate showCM)
        let f5 = canvas.Refresh 
        new Margin(textView, mgta, showCM,
            f1 >> f2 >> f3 >> f4 >> f5,
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
