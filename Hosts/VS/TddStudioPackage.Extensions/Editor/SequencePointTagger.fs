namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

type SequencePointTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let syncContext = SynchronizationContext.Current
    let tagsChanged = Event<_, _>()
    
    let fireTagsChanged _ = 
        Logger.logInfof "Firing SequencePointTagger.TagsChanged"
        syncContext.Send
            (SendOrPostCallback
                 (fun _ -> 
                 Common.safeExec 
                     (fun () -> 
                     tagsChanged.Trigger
                         (self, 
                          SnapshotSpanEventArgs
                              (new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), null)
    
    do dataStore.SequencePointsUpdated.Add fireTagsChanged
    interface ITagger<SequencePointTag> with
        
        (* NOTE: We are assuming that 
           (1) spans arg has only 1 item and it is a full line in the editor
           (2) Returned TagSpan.Span is the full span, i.e. it is not the set of intersection ranges of Span with SequencePoint *)
        member __.GetTags(spans : _) : _ = 
            let getMarkerTags _ p = 
                let findSPForSpan (sps : SequencePoint seq) (ss : SnapshotSpan) = 
                    sps 
                    |> Seq.where (fun sp -> 
                        let (sl, sc, el, ec) = 
                            ss.Start.GetContainingLine().LineNumber + 1, ss.Start.Difference(ss.Start) + 1, 
                            ss.End.GetContainingLine().LineNumber + 1, ss.Start.Difference(ss.End) + 1 - 1
                        sp.startLine <= DocumentCoordinate sl
                        && sp.endLine >= DocumentCoordinate el)

                let sps = p |> dataStore.GetSequencePointsForFile
                spans
                |> Seq.collect (fun ss -> 
                       ss
                       |> findSPForSpan sps
                       |> Seq.map (fun sp -> ss, sp))
                |> Seq.map (fun (ss, sp) -> TagSpan<_>(SnapshotSpan(ss.Start, ss.Length), { spx = sp }) :> ITagSpan<_>)
            buffer.FilePath
            |> Option.fold getMarkerTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
