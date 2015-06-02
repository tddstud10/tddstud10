namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain

type TestMarkerTagger(buffer : ITextBuffer, dataStore : IDataStore) as t = 
    let tagsChanged = Event<_, _>()
    let fireTagsChanged _ = 
        tagsChanged.Trigger
            (t, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length)))
    do dataStore.TestCasesUpdated.Add fireTagsChanged
    
    let getFilePath (buffer : ITextBuffer) = 
        match buffer.Properties.TryGetProperty(typeof<ITextDocument>) with
        | true, x -> 
            match box x with
            | :? ITextDocument as textDocument -> 
                textDocument.FilePath
                |> FilePath
                |> Some
            | _ -> None
        | _ -> None
    
    interface ITagger<TestMarkerTag> with
        
        member __.GetTags(spans : _) : _ = 
            let getMarkerTags _ path = 
                let findByDocAndLineNum (dataStore : IDataStore) path n = 
                    path
                    |> dataStore.GetUnitTestsInDocument
                    |> Seq.tryFind (fun t -> t.LineNumber = n)
                spans
                |> Seq.filter (fun s -> not s.IsEmpty)
                |> Seq.map (fun s -> s, findByDocAndLineNum dataStore path (s.Start.GetContainingLine().LineNumber + 1))
                |> Seq.choose (fun (s, t) -> t |> Option.bind (fun t -> Some(s, t)))
                |> Seq.map
                       (fun (s, t) -> 
                       new TagSpan<_>(new SnapshotSpan(s.Start, s.Length), { testCase = t }) :> ITagSpan<_>)
            buffer
            |> getFilePath
            |> Option.fold getMarkerTags Seq.empty
        
        [<CLIEvent>]
        member __.TagsChanged = tagsChanged.Publish
