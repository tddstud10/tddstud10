namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

type CodeCoverageTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let syncContext = SynchronizationContext.Current
    let tagsChanged = Event<_, _>()
    let fireTagsChanged _ = 
        Logger.logInfof "Firing CodeCoverageTagger.TagsChanged"
        syncContext.Send
            (SendOrPostCallback
                 (fun _ -> 
                 Common.safeExec 
                     (fun () -> 
                     tagsChanged.Trigger
                         (self, 
                          SnapshotSpanEventArgs
                              (new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), null)
    do dataStore.CoverageInfoUpdated.Add fireTagsChanged
    interface ITagger<CodeCoverageTag> with
        
        member __.GetTags(spans : _) : _ = 
            let getMarkerTags _ path = 
                //spans
                // TODO: If this crashes, fix unit tests and enable back
                //|> Seq.filter (fun s -> not s.IsEmpty)
                Seq.empty
            buffer.FilePath |> Option.fold getMarkerTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
