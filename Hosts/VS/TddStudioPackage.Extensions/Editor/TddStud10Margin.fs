namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open System

module TddStud10MarginConstants = 
    [<Literal>]
    let Name = R4nd0mApps.TddStud10.Constants.ProductName + " Margin"

type TddStud10Margin(textView : IWpfTextView) = 
    let mutable disposed = false
    override x.Finalize() = x.Dispose(false)
    
    member private x.Dispose(disposing : bool) = 
        if not disposed then 
            if (disposing) then 
                ()
            disposed <- true
    
    member private x.ThrowIfDisposed() =
        if disposed then
            raise (new ObjectDisposedException(TddStud10MarginConstants.Name))

    interface IDisposable with
        member x.Dispose() : unit = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
    
    interface ITextViewMargin with
        member x.Enabled : bool = 
            x.ThrowIfDisposed()
            false
        
        member x.GetTextViewMargin(marginName : string) : ITextViewMargin = 
            if marginName = TddStud10MarginConstants.Name then x :> ITextViewMargin
            else null
        
        member x.MarginSize : float = 
            x.ThrowIfDisposed()
            failwith "Not implemented yet"
    
    interface IWpfTextViewMargin with
        member x.VisualElement : Windows.FrameworkElement = 
            x.ThrowIfDisposed()
            failwith "Not implemented yet"
