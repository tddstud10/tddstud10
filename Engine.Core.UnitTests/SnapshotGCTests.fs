module R4nd0mApps.TddStud10.Engine.Core.SnapshotGCTests

open FsUnit.Xunit
open R4nd0mApps.TddStud10.Common.Domain
open System.IO
open System.Threading.Tasks
open global.Xunit

let (</>) a b = Path.Combine(a, b)

let runTest t : Task = 
    async { 
        let rootDir = Path.GetTempPath() </> Path.GetRandomFileName()
        Directory.CreateDirectory(rootDir) |> ignore
        do! t rootDir
        Directory.Delete(rootDir, true)
    }
    |> Async.StartAsTask :> _

[<Fact>]
let ``Should delete unmarked folder``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            garbage
            |> Seq.sort
            |> Seq.toList
            |> should matchList [ rootDir </> "A" ]
            garbage
            |> Seq.forall (Directory.Exists >> not)
            |> should be True
        }

[<Fact>]
let ``Should not delete fresh folder``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            SnapshotGC.mark (FilePath <| (rootDir </> "A"))
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            garbage
            |> Seq.sort
            |> Seq.toList
            |> should be Empty
            garbage
            |> Seq.forall Directory.Exists
            |> should be True
        }

[<Fact>]
let ``Should delete stale folder``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            SnapshotGC.unmark (FilePath <| (rootDir </> "A"))
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            garbage
            |> Seq.sort
            |> Seq.toList
            |> should matchList [ rootDir </> "A" ]
            garbage
            |> Seq.forall (Directory.Exists >> not)
            |> should be True
        }

[<Fact>]
let ``Should delete all stale folders``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            Directory.CreateDirectory(rootDir </> "B") |> ignore
            SnapshotGC.unmark (FilePath(rootDir </> "B"))
            Directory.CreateDirectory(rootDir </> "C") |> ignore
            SnapshotGC.mark (FilePath(rootDir </> "C"))
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            garbage
            |> Seq.sort
            |> Seq.toList
            |> should matchList [ rootDir </> "A"
                                  rootDir </> "B" ]
            garbage
            |> Seq.forall (Directory.Exists >> not)
            |> should be True
            [ rootDir </> "C" ]
            |> Seq.forall Directory.Exists
            |> should be True
        }
