namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EditorFrameworkExtensions

type TestMarkerTagger(buffer : ITextBuffer, dataStore : IDataStore) as self = 
    let tagsChanged = Event<_, _>()
    let fireTagsChanged _ = 
        tagsChanged.Trigger
            (self, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length)))
    do dataStore.TestCasesUpdated.Add fireTagsChanged
    interface ITagger<TestMarkerTag> with
        
        // TT
        member __.GetTags(spans : _) : _ = 
            let getMarkerTags _ path = 
                spans
                |> Seq.filter (fun s -> not s.IsEmpty)
                |> Seq.map (fun s -> s, DocumentCoordinate(s.Start.GetContainingLine().LineNumber + 1))
                |> Seq.map (fun (s, ln) -> s, dataStore.FindTestByDocumentAndLineNumber path ln)
                |> Seq.choose (fun (s, t) -> t |> Option.bind (fun t -> Some(s, t)))
                |> Seq.map 
                       (fun (s, t) -> 
                       new TagSpan<_>(new SnapshotSpan(s.Start, s.Length), { testCase = t }) :> ITagSpan<_>)
            buffer.getFilePath() |> Option.fold getMarkerTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
