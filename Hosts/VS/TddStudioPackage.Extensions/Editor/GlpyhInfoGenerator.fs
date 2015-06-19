namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

module GlyphInfoGenerator = 
    open System.ComponentModel.Design
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions
    open System
    open System.Windows
    open System.Windows.Media
    open Microsoft.VisualStudio.Text.Tagging
    
    let generate ((b, tags) : Rect * seq<IMappingTagSpan<IMarginGlyphTag>>) = 
        if tags |> Seq.isEmpty then None
        else 
            let gi = 
                { color = Colors.Green
                  glyphType = TestStart
                  glyphTag = 
                      tags
                      |> Seq.map (fun t -> t.Tag)
                      |> Seq.nth 0
                  toolTipText = ""
                  contextMenu = CommandID(Guid(PkgGuids.GuidGlyphContextCmdSet), PkgCmdID.GlyphContextMenu |> int) }
            (b, gi) |> Some
