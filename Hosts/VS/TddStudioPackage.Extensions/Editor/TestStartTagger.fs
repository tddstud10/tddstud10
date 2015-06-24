namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

type TestStartTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let syncContext = SynchronizationContext.Current
    let tagsChanged = Event<_, _>()
    
    let fireTagsChanged _ = 
        Logger.logInfof "Firing TestStartTagger.TagsChanged"
        syncContext.Send
            (SendOrPostCallback
                 (fun _ -> 
                 Common.safeExec 
                     (fun () -> 
                     tagsChanged.Trigger
                         (self, 
                          SnapshotSpanEventArgs(SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), 
             null)
    
    do dataStore.TestCasesUpdated.Add fireTagsChanged
    interface ITagger<TestStartTag> with
        
        member __.GetTags(spans : _) : _ = 
            let getTags _ path = 
                spans
                |> Seq.map (fun s -> 
                       s, 
                       { document = path
                         line = DocumentCoordinate(s.Start.GetContainingLine().LineNumber + 1) })
                |> Seq.map (fun (s, dl) -> s, dataStore.FindTest dl)
                |> Seq.filter (fun (_, ts) -> 
                       ts
                       |> Seq.isEmpty
                       |> not)
                |> Seq.map 
                       (fun (s, ts) -> 
                       TagSpan<_>(SnapshotSpan(s.Start, s.Length), 
                                  { testCases = ts
                                    location = 
                                        { document = path
                                          line = DocumentCoordinate(s.Start.GetContainingLine().LineNumber + 1) } }) :> ITagSpan<_>)
            buffer.FilePath |> Option.fold getTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
