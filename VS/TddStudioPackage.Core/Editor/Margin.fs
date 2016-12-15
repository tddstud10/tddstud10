namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open Microsoft.VisualStudio.Shell.Interop
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System

type Margin(textView : IWpfTextView, mgta : ITagAggregator<_>, painter, getMarginSize, getVisualElement) = 
    let mutable disposed = false
    
    let throwIfDisposed() = 
        if disposed then raise (ObjectDisposedException(MarginConstants.Name))
    
    let paintGlyphs () = 
        throwIfDisposed()
        (textView.ViewportLocation, textView.TextViewLines :> _ seq) |> painter
    
    let lcSub = textView.LayoutChanged.Subscribe(fun _ -> paintGlyphs())
    let tcSub = mgta.TagsChanged.Subscribe(fun _ -> paintGlyphs())
    let zlcSub = textView.ZoomLevelChanged.Subscribe(fun _ -> paintGlyphs())
    
    static member Create (dte : EnvDTE.DTE) (dbg : IVsDebugger3) (textView : IWpfTextView) (mgta : ITagAggregator<_>) = 
        (* NOTE: Pure wireup code in this constructor. Hence not tested. *)
        let getZL = fun () -> textView.ZoomLevel / 100.0
        let createHA = createHostActions dte dbg
        let canvas = MarginCanvas(getZL)
        
        let painter = 
            GlpyhBoundsGenerator.generate getZL
            >> (Seq.map (fun (b, l : ITextViewLine) -> b, l.Extent |> mgta.GetTags))
            >> (Seq.choose GlyphInfoGenerator.generate)
            >> (Seq.map (GlyphGenerator.generate createHA getZL))
            >> canvas.Refresh
        new Margin(textView, mgta, painter, (fun () -> canvas.UserControl.ActualWidth), (fun () -> canvas.UserControl))
    
    override x.Finalize() = x.Dispose(false)
    
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                lcSub.Dispose()
                tcSub.Dispose()
                zlcSub.Dispose()
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
