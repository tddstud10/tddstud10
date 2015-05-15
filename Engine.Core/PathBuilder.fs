namespace R4nd0mApps.TddStud10.Engine.Core

open System
open R4nd0mApps.TddStud10.Engine.Diagnostics
open System.IO
open R4nd0mApps.TddStud10

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
    
    let private normalizeAndCompare slnPath (path1 : string) (path2 : string) = 
        let d = Path.GetDirectoryName(Path.GetDirectoryName(slnPath))
        path1.ToUpperInvariant().Replace(snapShotRoot.ToUpperInvariant(), "").Trim('\\')
            .Equals(path2.ToUpperInvariant().Replace(d.ToUpperInvariant(), "").Trim('\\'), 
                    StringComparison.InvariantCultureIgnoreCase)

    let arePathsTheSame slnPath (path1 : string) (path2 : string) = 
        path1.ToUpperInvariant().Equals(path2.ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase) 
        || normalizeAndCompare slnPath path1 path2
        || normalizeAndCompare slnPath path2 path1
