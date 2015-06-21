namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

module GlyphInfoGenerator = 
    open System.ComponentModel.Design
    open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions
    open System
    open System.Windows
    open System.Windows.Media
    open Microsoft.VisualStudio.Text.Tagging
    
    let generate ((b, tags) : Rect * seq<IMappingTagSpan<IMarginGlyphTag>>) = 
        let tg = 
            tags
            |> Seq.map (fun t -> t.Tag)
            |> Seq.groupBy (fun t -> 
                   match t with
                   | :? TestStartTag -> TestStart
                   | :? SequencePointTag -> SequencePoint
                   | :? FailurePointTag -> FailurePoint
                   | _ -> failwith "Unknown IMarginTag type")
            |> dict
        
        (* NOTE: Many things wrong with this implementation:
           1. Incorrect use of the match statement 
           2. Tags - should they contain the matching tag or all the tags? *)
        let gt = 
            match tg with
            | tg when tg.Count = 0 -> None
            | tg when TestStart |> tg.ContainsKey -> (TestStart, tg.[TestStart], Colors.Green) |> Some
            | tg when FailurePoint |> tg.ContainsKey -> (FailurePoint, tg.[FailurePoint], Colors.Red) |> Some
            | tg when SequencePoint |> tg.ContainsKey -> (SequencePoint, tg.[SequencePoint], Colors.WhiteSmoke) |> Some
            | _ -> None
        
        gt |> Option.map (fun (t, ts, c) -> 
                  b, 
                  { color = c
                    glyphType = t
                    glyphTags = ts
                    toolTipText = ""
                    contextMenu = CommandID(Guid(PkgGuids.GuidGlyphContextCmdSet), PkgCmdID.GlyphContextMenu |> int) })
