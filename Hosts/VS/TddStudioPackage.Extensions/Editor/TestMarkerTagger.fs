namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core

type TestMarkerTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let syncContext = SynchronizationContext.Current
    let tagsChanged = Event<_, _>()
    let fireTagsChanged _ = 
        syncContext.Send
            (SendOrPostCallback
                 (fun _ -> 
                 Common.safeExec 
                     (fun () -> 
                     tagsChanged.Trigger
                         (self, 
                          SnapshotSpanEventArgs
                              (new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), null)
    do dataStore.TestCasesUpdated.Add fireTagsChanged
    interface ITagger<TestMarkerTag> with
        
        member __.GetTags(spans : _) : _ = 
            let getMarkerTags _ path = 
                spans
                // TODO: If this crashes, fix unit tests and enable back
                //|> Seq.filter (fun s -> not s.IsEmpty)
                |> Seq.map (fun s -> s, DocumentCoordinate(s.Start.GetContainingLine().LineNumber + 1))
                |> Seq.map (fun (s, ln) -> s, dataStore.FindTest2 path ln)
                |> Seq.collect (fun (s, ts) -> ts |> Seq.map (fun t -> s, t))
                |> Seq.map (fun (s, t) -> TagSpan<_>(SnapshotSpan(s.Start, s.Length), { testCase = t }) :> ITagSpan<_>)
            () |> buffer.getFilePath |> Option.fold getMarkerTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
