namespace R4nd0mApps.TddStud10.Common

open System
open System.IO
open R4nd0mApps.TddStud10
open R4nd0mApps.TddStud10.Common.Domain

module PathBuilder = 
    let snapShotRoot = Constants.SnapshotRoot
    
    let private makeSlnParentDirName slnPath = 
        match Path.GetFileName(Path.GetDirectoryName(slnPath)) with
        | "" -> Path.GetFileNameWithoutExtension(slnPath)
        | dn -> dn
    
    let makeSlnSnapshotPath (FilePath slnPath) = 
        let slnFileName = Path.GetFileName(slnPath)
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName, slnFileName))
    
    let makeSlnBuildRoot (FilePath slnPath) = 
        let slnParentDirName = makeSlnParentDirName slnPath
        FilePath(Path.Combine(snapShotRoot, slnParentDirName + ".out"))
    
    let private normalizeAndCompare (FilePath slnPath) (FilePath path1) (FilePath path2) = 
        let d = 
            slnPath
            |> Path.GetDirectoryName
            |> Path.GetDirectoryName
        path1.ToUpperInvariant().Replace(snapShotRoot.ToUpperInvariant(), "").Trim('\\')
             .Equals(path2.ToUpperInvariant().Replace(d.ToUpperInvariant(), "").Trim('\\'), 
                     StringComparison.InvariantCultureIgnoreCase)
    
    let arePathsTheSame slnPath path1 path2 = 
        path1 = path2 || normalizeAndCompare slnPath path1 path2 || normalizeAndCompare slnPath path2 path1
    
    let rebaseCodeFilePath rsp (FilePath p) = 
        let (FilePath slnPath) = rsp.solutionPath
        let (FilePath slnSnapPath) = rsp.solutionSnapshotPath
        p.ToUpperInvariant()
         .Replace(Path.GetDirectoryName(slnSnapPath).ToUpperInvariant(), 
                  Path.GetDirectoryName(slnPath).ToUpperInvariant()) |> FilePath
