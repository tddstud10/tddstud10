namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System
open Microsoft.VisualStudio.Text.Editor
open Microsoft.VisualStudio.Text.Tagging
open System.Windows.Shapes
open System.Windows.Controls
open System.Windows.Media
open Microsoft.VisualStudio.Text

type Margin(textView : IWpfTextView, tmta : ITagAggregator<TestMarkerTag>) = 
    let mutable disposed = false
    let canvas = new MarginCanvas(MarginConstants.Width)
    let buffer = textView.TextBuffer
    let textViewLayoutChanged _ (ea : TextViewLayoutChangedEventArgs) = 
        canvas.Children.Clear()
        textView.TextViewLines
        |> Seq.map (fun l -> l.Extent)
        |> Seq.filter (fun ss -> not ss.IsEmpty)
        |> Seq.map (fun ss -> tmta.GetTags(ss))
        |> Seq.collect id
        |> Seq.iter 
            (fun t -> 
                //System.Diagnostics.Debug.WriteLine(sprintf "#### " t.Span.)
                let line = textView.TextViewLines.[t.Tag.testCase.LineNumber - 1]
                let ellipse = new Ellipse()
                ellipse.Stroke <- new SolidColorBrush(Colors.Green);
                ellipse.StrokeThickness <- 1.5;
                ellipse.Height <- 8.0;
                ellipse.Width <- 8.0;
                ellipse.SetValue(Canvas.TopProperty, (line.Top + line.Bottom) / 2.0 - 4.0 - textView.ViewportTop);
                ellipse.SetValue(Canvas.LeftProperty, 1.0);
                canvas.Children.Add(ellipse) |> ignore)
    let lceh = new EventHandler<_>(textViewLayoutChanged)
    
    let testMarkerTagsChanged _ (ea : TagsChangedEventArgs) =
        canvas.Dispatcher.Invoke(
            fun () ->
                canvas.Children.Clear()
                textView.TextViewLines
                |> Seq.map (fun l -> l.Extent)
                |> Seq.filter (fun ss -> not ss.IsEmpty)
                |> Seq.map (fun ss -> tmta.GetTags(ss))
                |> Seq.collect id
                |> Seq.iter 
                    (fun t -> 
                        let line = textView.TextViewLines.[t.Tag.testCase.LineNumber - 1]
                        let ellipse = new Ellipse()
                        ellipse.Stroke <- new SolidColorBrush(Colors.Green);
                        ellipse.StrokeThickness <- 1.5;
                        ellipse.Height <- 8.0;
                        ellipse.Width <- 8.0;
                        ellipse.SetValue(Canvas.TopProperty, (line.Top + line.Bottom) / 2.0 - 4.0 - textView.ViewportTop);
                        ellipse.SetValue(Canvas.LeftProperty, 1.0);
                        canvas.Children.Add(ellipse) |> ignore))
    
    let tmtceh = new EventHandler<_>(testMarkerTagsChanged)
    
    do 
        textView.LayoutChanged.AddHandler(lceh)
        tmta.TagsChanged.AddHandler(tmtceh)
    
    override x.Finalize() = x.Dispose(false)
    
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                tmta.TagsChanged.RemoveHandler(tmtceh)
                textView.LayoutChanged.RemoveHandler(lceh)
            disposed <- true
    
    member private __.ThrowIfDisposed() = 
        if disposed then raise (new ObjectDisposedException(MarginConstants.Name))
    
    interface IDisposable with
        member x.Dispose() : _ = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
    
    interface ITextViewMargin with
        
        member x.Enabled : _ = 
            x.ThrowIfDisposed()
            true
        
        member x.GetTextViewMargin(marginName : _) : _ = 
            if marginName = MarginConstants.Name then x :> _
            else null
        
        member x.MarginSize : _ = 
            x.ThrowIfDisposed()
            canvas.ActualWidth
    
    interface IWpfTextViewMargin with
        member x.VisualElement : _ = 
            x.ThrowIfDisposed()
            canvas :> _
