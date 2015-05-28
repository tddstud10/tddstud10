namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open System

type Margin(textView : IWpfTextView) = 
    let mutable disposed = false
    let canvas = new MarginCanvas(MarginConstants.Width)
    let textViewLayoutChanged o ea = ()
    let lceh = new EventHandler<_>(textViewLayoutChanged)
    do textView.LayoutChanged.AddHandler(lceh)
    override x.Finalize() = x.Dispose(false)
    
    member private x.Dispose(disposing : bool) = 
        if not disposed then 
            if (disposing) then textView.LayoutChanged.RemoveHandler(lceh)
            disposed <- true
    
    member private x.ThrowIfDisposed() = 
        if disposed then raise (new ObjectDisposedException(MarginConstants.Name))
    
    interface IDisposable with
        member x.Dispose() : unit = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
    
    interface ITextViewMargin with
        
        member x.Enabled : bool = 
            x.ThrowIfDisposed()
            true
        
        member x.GetTextViewMargin(marginName : string) : ITextViewMargin = 
            if marginName = MarginConstants.Name then x :> _
            else null
        
        member x.MarginSize : float = 
            x.ThrowIfDisposed()
            canvas.ActualWidth
    
    interface IWpfTextViewMargin with
        member x.VisualElement : Windows.FrameworkElement = 
            x.ThrowIfDisposed()
            canvas :> _
