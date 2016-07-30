$root = Split-Path $PSScriptRoot

Write-Host "Rename all assemblies"
dir $root -rec -filt *.*proj | % {
    $project = [xml](gc $_.FullName)
    $project.Project.PropertyGroup | ? { $_.AssemblyName } | % { $_.AssemblyName += '.df' }
    $project.Save($_.FullName)
}

Write-Host "Change Constants.fs and Constants.cs"
[System.IO.File]::WriteAllText("$root\Common\Constants.cs", @"
namespace R4nd0mApps.TddStud10
{
    internal class Constants
    {
        public const string ProductVariant = ".df";

        public const string ProductName = "Test Driven Development Studio (dogfood)";

        public const string RealTimeSessionName = "R4nd0mApps-TddStud10-Realtime-Session-df";
        public const string EtwProviderNameHostsVS = "R4nd0mApps-TddStud10-Hosts-VS-df";
        public const string EtwProviderNameHostsConsole = "R4nd0mApps-TddStud10-Hosts-Console-df";
        public const string EtwProviderNameTestHost = "R4nd0mApps-TddStud10-TestHost-df";
        public const string EtwProviderNameEngine = "R4nd0mApps-TddStud10-Engine-df";
        public const string EtwProviderNameTestRuntime = "R4nd0mApps-TddStud10-TestRuntime-df";
    }
}
"@)
[System.IO.File]::WriteAllText("$root\Common\Constants.fs", @"
namespace R4nd0mApps.TddStud10

module internal Constants =

    [<Literal>]
    let ProductVariant = ".df";

    [<Literal>]
    let ProductName = "Test Driven Development Studio (dogfood)";

    [<Literal>]
    let RealTimeSessionName = "R4nd0mApps-TddStud10-Realtime-Session-df";
    [<Literal>]
    let EtwProviderNameHostsVS = "R4nd0mApps-TddStud10-Hosts-VS-df";
    [<Literal>]
    let EtwProviderNameHostsConsole = "R4nd0mApps-TddStud10-Hosts-Console-df";
    [<Literal>]
    let EtwProviderNameTestHost = "R4nd0mApps-TddStud10-TestHost-df";
    [<Literal>]
    let EtwProviderNameEngine = "R4nd0mApps-TddStud10-Engine-df";
    [<Literal>]
    let EtwProviderNameTestRuntime = "R4nd0mApps-TddStud10-TestRuntime-df";
"@)

Write-Host "Change strings in resx files"
dir $root -rec -filt *.resx | % {
    $resx = [xml](gc $_.FullName)
    $resx.root.data | ? { $_ -and -not ($_.type -or $_.mimetype) } | % { $_.value += " (dogfood)" }
    $resx.Save($_.FullName)
}

Write-Host "Modifying relevant entries source.extension.vsixmanifest"
dir $root -rec -filt source.extension.vsixmanifest | % {
    $manifest = [xml](gc $_.FullName)
    $manifest.PackageManifest.Metadata.DisplayName += " (dogfood)"
    $manifest.Save($_.FullName)
}
