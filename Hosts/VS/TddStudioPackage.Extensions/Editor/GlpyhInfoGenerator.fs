module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.GlyphInfoGenerator

open System.ComponentModel.Design
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions
open System
open System.Windows
open System.Windows.Media
open Microsoft.VisualStudio.Text.Tagging
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain
open System.Collections.Generic

let tryGetTags<'T when 'T : equality and 'T :> IMarginGlyphTag> = 
    Dict.tryGetValue Seq.empty id typeof<'T> >> Seq.map (fun t -> box t :?> 'T)

let generate ((b, ts) : Rect * seq<IMappingTagSpan<IMarginGlyphTag>>) = 
    let gs = 
        ts
        |> Seq.map (fun t -> t.Tag)
        |> Seq.groupBy (fun t -> t.GetType())
        |> dict
    if gs.Count = 0 then None
    else 
        let color = 
            let ccts = 
                gs
                |> tryGetTags<CodeCoverageTag>
                |> Seq.map (fun t -> t.testResults)
                |> Seq.collect id
            match ccts with
            | ts when ts |> Seq.isEmpty -> Colors.Gray
            | ts when ts |> Seq.forall (fun t -> t.Outcome = TestOutcome.Passed) -> Colors.Green
            | _ -> Colors.Red
        
        let glyphType = 
            if not (gs
                    |> tryGetTags<TestStartTag>
                    |> Seq.isEmpty)
            then TestStart
            else if not (gs
                         |> tryGetTags<FailurePointTag>
                         |> Seq.isEmpty)
            then FailurePoint
            else if color = Colors.Gray then CodeCoveragePartial
            else CodeCoverageFull
        
        Some(b, 
             { color = color
               glyphType = glyphType
               glyphTags = ts |> Seq.map (fun t -> t.Tag)
               toolTipText = ""
               contextMenu = CommandID(Guid(PkgGuids.GuidGlyphContextCmdSet), PkgCmdID.GlyphContextMenu |> int) })
