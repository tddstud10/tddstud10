namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

(* NOTE: These are supposed to be 'never changing' stubs. Hence dont have unit tests.
   If they bugs start to show up and these need to be changed, add unit tests and move 
   to another appropriate location before changing.
 *)

[<AutoOpen>]
module SnapshotSpanExtensions = 
    open Microsoft.VisualStudio.Text
    
    type SnapshotSpan with
        member self.Bounds1Based = 
            let s, e = self.Start, self.End
            s.GetContainingLine().LineNumber + 1, s.Difference(s) + 1, e.GetContainingLine().LineNumber + 1, 
            s.Difference(e) + 1 - 1

[<AutoOpen>]
module ITextViewLineExtensions = 
    open System.Windows
    open Microsoft.VisualStudio.Text.Formatting
    
    type ITextViewLine with
        member self.Bounds = new Rect(self.Left, self.Top, self.Width, self.Height)

[<AutoOpen>]
module ITextViewExtensions = 
    open System.Windows
    open Microsoft.VisualStudio.Text.Editor
    
    type ITextView with
        member self.ViewportLocation = new Point(self.ViewportLeft, self.ViewportTop)

[<AutoOpen>]
module ITextBufferExtensions = 
    open Microsoft.VisualStudio.Text
    open R4nd0mApps.TddStud10.Common.Domain
    
    type ITextBuffer with

        member self.FilePath = 
            let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger
            let p = 
                match self.Properties.TryGetProperty(typeof<ITextDocument>) with
                | true, x -> 
                    match box x with
                    | :? ITextDocument as textDocument -> 
                        textDocument.FilePath
                        |> FilePath
                        |> Some
                    | _ -> None
                | _ -> None
            if p = None then logger.logErrorf "Buffer does not have ITextDocument property. Cannot get filename."
            p

[<AutoOpen>]
module ITagAggregatorExtensions = 
    open Microsoft.VisualStudio.Text
    open Microsoft.VisualStudio.Text.Tagging
    
    type SnapshotSnapsToTagSpan<'T when 'T :> ITag> = NormalizedSnapshotSpanCollection -> seq<TagSpan<'T>>
    
    type ITagAggregator<'T when 'T :> ITag> with
        member self.getTagSpans : SnapshotSnapsToTagSpan<'T> = 
            fun snapshotSpans -> 
                snapshotSpans
                |> Seq.collect (fun (s : SnapshotSpan) -> 
                       s
                       |> self.GetTags
                       |> Seq.map (fun mts -> s, mts))
                |> Seq.collect (fun (s, mts) -> mts.Span.GetSpans(s.Snapshot) |> Seq.map (fun s -> TagSpan(s, mts.Tag)))
