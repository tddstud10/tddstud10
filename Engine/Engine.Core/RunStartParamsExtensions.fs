namespace R4nd0mApps.TddStud10.Engine.Core

[<AutoOpen>]
module RunStartParamsExtensions = 
    open R4nd0mApps.TddStud10.Common
    open System
    open System.IO
    open System.Reflection
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10
    
    let private getLocalPath() = 
        (Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
        |> Path.GetFullPath
        |> Path.GetDirectoryName
    
    let testHostProcessName = sprintf "TddStud10.TestHost%s.exe" Constants.ProductVariant
    
    type RunStartParams with
        static member Create (cfg : EngineConfig) startTime solutionPath = 
            let snapShotRoot = Environment.ExpandEnvironmentVariables(cfg.SnapShotRoot) |> FilePath
            let buildRoot = PathBuilder.makeSlnBuildRoot snapShotRoot solutionPath
            { SnapShotRoot = snapShotRoot
              StartTime = startTime
              TestHostPath = Path.Combine(() |> getLocalPath, testHostProcessName) |> FilePath
              Solution = 
                  { Path = solutionPath
                    SnapshotPath = PathBuilder.makeSlnSnapshotPath snapShotRoot solutionPath
                    BuildRoot = buildRoot }
              IgnoredTests = cfg.IgnoredTests
              AdditionalMSBuildProperties = cfg.AdditionalMSBuildProperties
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
