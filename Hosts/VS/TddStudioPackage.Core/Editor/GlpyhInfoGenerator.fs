module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlyphInfoGenerator

open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open System.Windows
open System.Windows.Media

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
                |> Seq.map (fun t -> t.CctTestResults)
                |> Seq.collect id
            match ccts with
            | ts when ts |> Seq.isEmpty -> Colors.LightGray
            | ts when ts |> Seq.forall (fun t -> t.Outcome = DTestOutcome.TOPassed) -> Colors.Green
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
            else CodeCoverage
        
        Some(b, 
             { Color = color
               Type = glyphType
               Tags = ts |> Seq.map (fun t -> t.Tag) })
