// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing
open System
open System.IO

MSBuildDefaults <- { MSBuildDefaults with Verbosity = Some MSBuildVerbosity.Minimal }

// Directories
let buildDir  = @".\build\"
let testDir  = @".\build\"
let nugetDir = @".\NuGet\"
ensureDirExists (directoryInfo nugetDir)

// Filesets
let solutionFile = "TddStud10.sln"

// version info
let version = if buildServer = BuildServer.AppVeyor then AppVeyor.AppVeyorEnvironment.BuildVersion else "1.0.0.0"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; nugetDir]
)

Target "Rebuild" DoNothing

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

    // AppVeyor workaround
    !! @"packages\Newtonsoft.Json\lib\net45\Newtonsoft.Json.dll"
    |> CopyFiles buildDir
)


let runTest pattern =
    fun _ ->
        !! (buildDir + pattern)
        |> xUnit (fun p ->
            { p with
                ToolPath = findToolInSubPath "xunit.console.exe" (currentDirectory @@ "tools" @@ "xUnit")
                WorkingDir = Some testDir })

Target "Test" DoNothing
Target "UnitTests" (runTest "/*.UnitTests*.dll")
Target "ContractTests" 
    (if File.Exists(sprintf @"%s\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe" <| environVar "ProgramFiles(x86)") then 
        runTest "/*.ContractTests*.dll" 
    else 
        fun () -> traceImportant "Not required to run ContractTests on VS2013 boxes for simplifying test matrix.")


Target "Package" (fun _ ->
    let buildDirRel = sprintf @"..\build\%s"
    let exclusions = 
        "*.pdb;*.nupkg;*.nupkg.zip;*.vsix;Microsoft.VisualStudio.*;*.*Tests.dll;*.xml;*.lastcodeanalysissucceeded;approval*;xunit.*;envdte*;galasoft.*"
        |> fun e -> e.Split([|';'|])
        |> Array.map buildDirRel
        |> fun es -> String.Join(";", es)

    "TddStud10.nuspec"
    |> NuGet (fun p -> 
        { p with               
            Authors = [ "The TddStud10 Team" ]
            Project = "TddStud10.Core"
            Description = "Core TddStud10 Runtime and Libraries"
            Version = version
            Files = [ buildDirRel "*.*", Some "bin", Some exclusions
                      buildDirRel "amd64\*.*", Some @"bin\amd64", None
                      buildDirRel "x86\*.*", Some @"bin\x86", None ]
            OutputPath = buildDir })
)

Target "Publish" (fun _ ->
    !! "build\*.nupkg"
    |> AppVeyor.PushArtifacts
)

"Clean" ?=> "Build"
"Clean" ==> "Rebuild" 
"Build" ==> "Rebuild" 
"Build" ?=> "UnitTests" ==> "Test"
"Build" ?=> "ContractTests" ==> "Test"
"Rebuild" ==> "Test"
"Test" ==> "Package" ==> "Publish"

// start build
RunTargetOrDefault "Test"
