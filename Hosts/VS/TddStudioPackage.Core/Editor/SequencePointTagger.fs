namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

type SequencePointTagger(buffer : ITextBuffer, dataStore : IDataStore) = 
    let tagsChanged = Event<_, _>()
    interface ITagger<SequencePointTag> with
        
        (* NOTE: We are assuming that 
           (1) spans arg has only 1 item and it is a full line in the editor
           (2) Returned TagSpan.Span is the full span, i.e. it is not the set of intersection ranges of Span with SequencePoint *)
        member __.GetTags(spans : _) : _ = 
            let getTags _ p = 
                let findSPForSpan (sps : SequencePoint seq) (ss : SnapshotSpan) = 
                    sps |> Seq.where (fun sp -> 
                               let sl, _, el, _ = ss.Bounds1Based
                               sp.startLine <= DocumentCoordinate sl && sp.endLine >= DocumentCoordinate el)
                
                let sps = p |> dataStore.GetSequencePointsForFile
                spans
                |> Seq.collect (fun ss -> 
                       ss
                       |> findSPForSpan sps
                       |> Seq.map (fun sp -> ss, sp))
                |> Seq.map 
                       (fun (ss, sp) -> 
                       TagSpan<_>(SnapshotSpan(ss.Start, ss.Length), { SequencePointTag.sp = sp }) :> ITagSpan<_>)
            buffer.FilePath |> Option.fold getTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
