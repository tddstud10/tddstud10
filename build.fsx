// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

// Directories
let buildDir  = "./build/"
let testDir  = "./build/"

// Filesets
let solutionFile = "TddStud10.sln"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir]
)

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuild buildDir "Build"
         [
            "Configuration", "Release"
            "Platform", "Any CPU"
            "CreateVsixContainer", "true"
            "DeployExtension", "false"
            "CopyVsixExtensionFiles", "false"
         ]
    |> Log "Build-Output: "
)

Target "Test" (fun _ ->
    !! (buildDir + "/*.UnitTests.dll")
    |> xUnit (fun p ->
        { p with
            ToolPath = findToolInSubPath "xunit.console.exe" (currentDirectory @@ "tools" @@ "xUnit")
            WorkingDir = Some testDir })
)

"Clean" ==> "Build" ==> "Test"

// start build
RunTargetOrDefault "Test"
