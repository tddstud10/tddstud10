namespace R4nd0mApps.TddStud10.Common.Domain

[<AutoOpen>]
module RunStartParamsExtensions = 
    open R4nd0mApps.TddStud10.Common
    open System
    open System.IO
    open System.Reflection
    
    let private getLocalPath() = 
        (Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName
    
    let create startTime solutionPath = 
        let buildRoot = PathBuilder.makeSlnBuildRoot solutionPath
        { StartTime = startTime
          TestHostPath = Path.Combine(() |> getLocalPath, "TddStud10.TestHost.exe") |> FilePath
          Solution = 
              { Path = solutionPath
                SnapshotPath = PathBuilder.makeSlnSnapshotPath solutionPath
                BuildRoot = buildRoot }
          DataFiles = 
              { SequencePointStore =
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_sequencePointStore.xml" ]
                CoverageSessionStore = 
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_coverageresults.xml" ]
                TestResultsStore = 
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_testresults.xml" ]
                DiscoveredUnitTestsStore = 
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_discoveredUnitTests.xml" ]
                DiscoveredUnitDTestsStore = 
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_discoveredUnitDTests.xml" ]
                TestFailureInfoStore = 
                    PathBuilder.combine [ buildRoot
                                          FilePath "Z_testFailureInfo.xml" ] } }
