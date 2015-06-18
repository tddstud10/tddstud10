namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

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
    open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

    type ITextBuffer with
        member buffer.FilePath with get() = 
            let p = 
                match buffer.Properties.TryGetProperty(typeof<ITextDocument>) with
                | true, x -> 
                    match box x with
                    | :? ITextDocument as textDocument -> 
                        textDocument.FilePath
                        |> FilePath
                        |> Some
                    | _ -> None
                | _ -> None
            if p = None then Logger.logErrorf "Buffer does not have ITextDocument property. Cannot get filename."
            p
    