namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Windows
open Microsoft.VisualStudio.Text.Formatting
open Microsoft.VisualStudio.Shell.Interop
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System.Collections.Generic
open Microsoft.VisualStudio.Text
open GlyphGenerator


type Margin(textView : IWpfTextView, mgta : ITagAggregator<_>, painter, getMarginSize, getVisualElement) = 
    let mutable disposed = false
    
    let cache = Dictionary<SnapshotSpan, IEnumerable<IMappingTagSpan<IMarginGlyphTag>>>()

    let throwIfDisposed() = 
        if disposed then raise (ObjectDisposedException(MarginConstants.Name))
    
    let paintGlyphs cache = 
        throwIfDisposed()
        (textView.ViewportLocation, textView.TextViewLines :> _ seq) |> painter cache
    
    let lcSub = textView.LayoutChanged.Subscribe(fun _ -> paintGlyphs cache)
    let tcSub = mgta.TagsChanged.Subscribe(fun _ -> cache.Clear(); paintGlyphs cache)
    let zlcSub = textView.ZoomLevelChanged.Subscribe(fun _ -> paintGlyphs cache)

    static member Create (dte: EnvDTE.DTE) (dbg: IVsDebugger3) (textView : IWpfTextView) (mgta : ITagAggregator<_>) = 
        (* NOTE: Pure wireup code in this constructor. Hence not tested. *)
        let getZL = fun() -> textView.ZoomLevel / 100.0
        let createHA = createHostActions dte dbg
        let canvas : MarginCanvas = 
            if XX.canvasV2 then
                MarginCanvasV2(getZL) :> _
            else
                MarginCanvasV1(getZL) :> _
                
        let painter cache = 
            GlpyhBoundsGenerator.generate getZL
            >> (Seq.map (fun (b, (l : ITextViewLine)) -> b, (XX.xxx cache l mgta)))
            >> (Seq.choose GlyphInfoGenerator.generate)
            >> (Seq.map (GlyphGenerator.generate createHA getZL))
            >> canvas.Refresh 
        new Margin(textView, mgta, painter, (fun () -> canvas.FE.ActualWidth), (fun () -> canvas.FE))
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
