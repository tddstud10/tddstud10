namespace R4nd0mApps.TddStud10.Common.Domain

[<AutoOpen>]
module RunStartParamsExtensions = 
    open System.IO
    open System
    open System.Reflection
    open R4nd0mApps.TddStud10.Common
    
    let private getLocalPath() = 
        (Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName
        
    let create startTime solutionPath = 
        { startTime = startTime
          testHostPath = Path.Combine(() |> getLocalPath, "TddStud10.TestHost.exe") |> FilePath
          solutionPath = solutionPath
          solutionSnapshotPath = PathBuilder.makeSlnSnapshotPath solutionPath
          solutionBuildRoot = PathBuilder.makeSlnBuildRoot solutionPath }
