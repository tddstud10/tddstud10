namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

type FailurePointTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let syncContext = SynchronizationContext.Current
    let tagsChanged = Event<_, _>()
    
    let fireTagsChanged _ = 
        Logger.logInfof "Firing FailurePointTagger.TagsChanged"
        syncContext.Send
            (SendOrPostCallback
                 (fun _ -> 
                 Common.safeExec 
                     (fun () -> 
                     tagsChanged.Trigger
                         (self, 
                          SnapshotSpanEventArgs(SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), 
             null)
    
    do dataStore.TestFailureInfoUpdated.Add fireTagsChanged
    interface ITagger<FailurePointTag> with
        
        (* NOTE: We are assuming that 
           (1) spans arg has only 1 item and it is a full line in the editor
           (2) Returned TagSpan.Span is the full span, i.e. it is not the set of intersection ranges of Span with failure sequence point. *)
        member __.GetTags(spans : _) : _ = 
            let getTags _ path = 
                spans
                |> Seq.map (fun s -> 
                       s, 
                       { document = path
                         line = (s.Start.GetContainingLine().LineNumber + 1) |> DocumentCoordinate })
                |> Seq.map (fun (s, dl) -> s, dl |> dataStore.FindTestFailureInfo)
                |> Seq.where (fun (_, tfis) -> 
                       tfis
                       |> Seq.isEmpty
                       |> not)
                |> Seq.map 
                       (fun (s, tfis) -> TagSpan<_>(SnapshotSpan(s.Start, s.Length), { tfis = tfis }) :> ITagSpan<_>)
            buffer.FilePath |> Option.fold getTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
