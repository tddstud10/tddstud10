namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions
open System.Threading
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

type CodeCoverageTagger(buffer : ITextBuffer, getspt : SnapshotSnapsToTagSpan<SequencePointTag>, dataStore : IDataStore) as self = 
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
                          SnapshotSpanEventArgs(SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length))))), 
             null)
    
    do dataStore.SequencePointsUpdated.Add fireTagsChanged
    do dataStore.CoverageInfoUpdated.Add fireTagsChanged
    new(buffer : ITextBuffer, spta : ITagAggregator<SequencePointTag>, dataStore : IDataStore) = 
        new CodeCoverageTagger(buffer, spta.getTagSpans, dataStore)
    interface ITagger<CodeCoverageTag> with
        
        (* NOTE: We are assuming that 
           (1) spans arg has only 1 item and it is a full line in the editor
           (2) Returned TagSpan.Span is the full span, i.e. it is not the set of intersection ranges of Span with failure sequence point. *)
        member __.GetTags(spans : _) : _ = 
            let getTags _ _ = 
                spans
                |> getspt
                |> Seq.map (fun tsp -> tsp, tsp.Tag.sp.id |> dataStore.GetRunIdsForTestsCoveringSequencePointId)
                |> Seq.map (fun (tsp, rids) -> 
                       tsp, 
                       rids
                       |> Seq.map (fun rid -> rid.testId)
                       |> Seq.distinct
                       |> Seq.map dataStore.GetResultsForTestId
                       |> Seq.collect id)
                |> Seq.map (fun (tsp, trs) -> 
                       TagSpan<_>(tsp.Span, 
                                  { CodeCoverageTag.CCTSeqPoint = tsp.Tag.sp
                                    CCTTestResults = trs }) :> ITagSpan<_>)
            buffer.FilePath |> Option.fold getTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
