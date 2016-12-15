module R4nd0mApps.TddStud10.Engine.Core.SnapshotGCTests

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
            Assert.Equal<string[]>(garbage |> Seq.sort |> Seq.toArray, [| rootDir </> "A" |])
            Assert.True(garbage |> Seq.forall (Directory.Exists >> not))
        }

[<Fact>]
let ``Should not delete fresh folder``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            SnapshotGC.mark (FilePath <| (rootDir </> "A"))
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            Assert.Equal<string[]>(garbage |> Seq.sort |> Seq.toArray, [||])
            Assert.True(garbage |> Seq.forall Directory.Exists)
        }

[<Fact>]
let ``Should delete stale folder``() = 
    runTest <| fun rootDir -> 
        async { 
            Directory.CreateDirectory(rootDir </> "A") |> ignore
            SnapshotGC.unmark (FilePath <| (rootDir </> "A"))
            let! garbage = SnapshotGC.sweep (FilePath rootDir)
            Assert.Equal<string[]>(garbage |> Seq.sort |> Seq.toArray, [| rootDir </> "A" |])
            Assert.True(garbage |> Seq.forall (Directory.Exists >> not))
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
            Assert.Equal<string[]>(garbage |> Seq.sort |> Seq.toArray, 
                                   [| rootDir </> "A"
                                      rootDir </> "B" |])
            Assert.True(garbage |> Seq.forall (Directory.Exists >> not))
            Assert.True([ rootDir </> "C" ] |> Seq.forall Directory.Exists)
        }
