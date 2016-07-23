namespace R4nd0mApps.TddStud10.Common

open System.IO
open R4nd0mApps.TddStud10
open R4nd0mApps.TddStud10.Common.Domain

module PathBuilder = 
    let combine = 
        List.reduce (fun (FilePath acc) (FilePath e) -> Path.Combine(acc, e) |> FilePath)

    let getFileName (FilePath p) =
        Path.GetFileName(p) |> FilePath
    
    let enumerateFiles so filter (FilePath path) =
        Directory.EnumerateFiles(path, filter, so)
        |> Seq.map FilePath

    let fileExists (FilePath p) = File.Exists(p)

    let directoryExists (FilePath p) = Directory.Exists(p)

    let private makeSlnParentDirName slnPath = 
        match Path.GetFileName(Path.GetDirectoryName(slnPath)) with
        | "" -> Path.GetFileNameWithoutExtension(slnPath)
        | dn -> dn
    
    let makeSlnSnapshotPath (FilePath snapShotRoot) (FilePath slnPath) = 
        let slnFileName = Path.GetFileName(slnPath)
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName, slnFileName))
    
    let makeSlnBuildRoot (FilePath snapShotRoot) (FilePath slnPath) = 
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName, "out"))
    
    let rebaseCodeFilePath ((FilePath slnPath), (FilePath slnSnapPath)) (FilePath p) = 
        p.ToUpperInvariant()
         .Replace(Path.GetDirectoryName(slnSnapPath).ToUpperInvariant(), 
                  Path.GetDirectoryName(slnPath).ToUpperInvariant()) |> FilePath
