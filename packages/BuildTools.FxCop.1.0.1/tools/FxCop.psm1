<#
    BuildTools.FxCop - Copyright(c) 2013 - Jon Wagner
    See https://github.com/jonwagner/BuildTools for licensing and other information.
    Version: 1.0.1
    Changeset: 152ba98243f907d846194867439eab4d887d7fe1
#>

# define our variables
$thisFolder = (Split-Path $MyInvocation.MyCommand.Definition -Parent)
$packageID = 'BuildTools_FxCop'

# import the standard build module
$msbuildModule = (Join-Path $thisFolder BuildTools.MsBuild.psm1)
if (!(Test-Path $msbuildModule)) {
    # in dev mode, this file is not in this path
    $msbuildModule = (Join-Path $thisFolder ..\..\BuildTools.MsBuild\BuildTools.MsBuild.psm1)
}
Import-Module $msbuildModule -Force

<#
.Synopsis
    Installs FxCop into the given project.
.Description
    Installs FxCop into the given project.

    The default installation enables FxCop for Release builds and treats Errors as Errors.
    You can change the behavior by running Enable-FxCop or Set-FxCopWarningsAs.
.Parameter Project
    The project to modify.
.Example
    Install-FxCop $Project

    Installs FxCop into the given project.
#>
function Install-FxCop {
    param (
        $Project,
        [switch] $Quiet
    )

    # make sure that we always have an msbuild project
    $Project = Get-MsBuildProject $project

    if (!$Quiet) {
        Write-Host "Installing FxCop in $($Project.FullPath)"
    }

    if (!(Get-MsBuildProperty -Project $Project -Name BuildToolsFxCopVersion)) {

        # the first time in, set the ruleset, set warnings to errors, and enable for release mode
        Disable-FxCop -Project $Project -Quiet
        Set-MsBuildConfigurationProperty -Project $Project -Name CodeAnalysisRuleSet -Value 'CodeAnalysisRules.ruleset'
        Set-FxCopWarningsAs -Project $Project Errors -Quiet
        Enable-FxCop -Project $Project -Configuration Release -TreatWarningsAs Errors -Quiet

        # tag the installation with the version so we don't overwrite peoples' settings
        Set-MsBuildProperty -Project $Project -Name BuildToolsFxCopVersion -Value '1.0.1'
    }
    else {
        if (!$Quiet) {
            Write-Host 'BuildTools.FxCop was already installed. Not modifying settings.'
        }

        # restore the previous state
        Get-MsBuildConfiguration -Project $Project |% {
            $group = $_
            $group.Properties |? Name -eq 'RunCodeAnalysisRestore' |% {
                $group.SetProperty('RunCodeAnalysis', $_.Value) | Out-Null
            }
        }
    }
}

<#
.Synopsis
    Uninstalls FxCop from the given project.
.Description
    Uninstalls FxCop from the given project.
.Parameter Project
    The project to modify.
.Example
    Uninstall-FxCop $Project

    Uninstall FxCop from the given project.
#>
function Uninstall-FxCop {
    param (
        $Project,
        [switch] $Quiet
    )

    # make sure that we always have an msbuild project
    $Project = Get-MsBuildProject $project

    if (!$Quiet) {
        Write-Host "Uninstalling FxCop in $($Project.FullPath)"
    }

    # leave the FxCop variables around so our settings can survive an upgrade
    Get-MsBuildConfiguration -Project $Project |% {
        $value = $_.Properties |? Name -eq 'RunCodeAnalysis' |% Value
        $_.SetProperty('RunCodeAnalysisRestore', $value) | Out-Null
    }
    Disable-FxCop -Project $Project -Quiet:$Quiet
}

<#
.Synopsis
    Enables FxCop for a given project configuration.
.Description
    Enables FxCop for a given project configuration.

    If Configuration and Platform are not specified, this enables FxCop for all configurations.
    The Error setting is not modified uless TreatErrorsAs is specified.
.Parameter Project
    The project to modify.
.Parameter TreatWarningsAs
    Sets Warnings as Errors or Warnings for the given configurations.
.Parameter Configuration
    The configuration to modify (e.g. Debug or Release).
.Parameter Platform
    The platform configuration to modify (e.g. AnyCPU or x86)
.Parameter Quiet
    Suppresses installation messages.
.Example
    Enable-FxCop $Project

    Enables FxCop for all configurations.
.Example
    Enable-FxCop $Project -Configuration Release

    Enables FxCop for all Release configurations.
#>
function Enable-FxCop {
    param (
        $Project,
        [ValidateSet('Errors', 'Warnings')] [string] $TreatWarningsAs,
        [string] $Configuration,
        [string] $Platform,
        [switch] $Quiet
    )

    # make sure that we always have an msbuild project
    $Project = Get-MsBuildProject $project

    if (!$Quiet) {
        $whichConfig = $Configuration
        if (!$whichConfig) { $whichConfig = 'All Configurations' }
        $whichPlat = $Platform
        if (!$whichPlat) { $whichPlat = 'All Platforms' }
        Write-Host "Enabling FxCop in $($Project.FullPath) for $whichConfig $whichPlat"
    }

    # enable FxCop for the specified configurations
    Set-MsBuildConfigurationProperty -Project $Project `
        -Name RunCodeAnalysis -Value $true `
        -Configuration $Configuration -Platform $Platform

    # set errors as warnings/error if not null
    if ($TreatWarningsAs) {
        Set-FxCopWarningsAs $Project -TreatWarningsAs $TreatWarningsAs -Configuration $Configuration -Platform $Platform -Quiet:$Quiet
    }

    # add CODE_ANALYSIS to the constants so code suppression works
    Enable-CodeAnalysisConstant -Project $Project `
        -Configuration $Configuration -Platform $Platform
}

<#
.Synopsis
    Disables FxCop for a given project configuration.
.Description
    Disables FxCop for a given project configuration.

    If Configuration and Platform are not specified, this disables FxCop for all configurations.
.Parameter Project
    The project to modify.
.Parameter Configuration
    The configuration to modify (e.g. Debug or Release).
.Parameter Platform
    The platform configuration to modify (e.g. AnyCPU or x86)
.Parameter Quiet
    Suppresses installation messages.
.Example
    Disable-FxCop $Project

    Disables FxCop for all configurations.
.Example
    Disable-FxCop $Project -Configuration Debug

    Disables FxCop for all Debug configurations.
#>
function Disable-FxCop {
    param (
        $Project,
        [string] $Configuration,
        [string] $Platform,
        [switch] $Quiet
    )

    # make sure that we always have an msbuild project
    $Project = Get-MsBuildProject $project

    if (!$Quiet) {
        Write-Host "Disabling FxCop in $($Project.FullPath)"
    }

    # set FxCopEnabled to false (default is true)
    Set-MsBuildConfigurationProperty -Project $Project `
        -Name RunCodeAnalysis -Value $false `
        -Configuration $Configuration -Platform $Platform

    # remove CODE_ANALYSIS from the constants unless fxcop is enabled
    Disable-CodeAnalysisConstant -Project $Project `
        -Configuration $Configuration -Platform $Platform
}

<#
.Synopsis
    Sets FxCop Errors as compile Errors or Warnings.
.Description
    Sets FxCop Errors as compile Errors or Warnings.
.Parameter TreatErrorsAs
    Sets Errors as Errors or Warnings for the given configurations.
.Parameter Project
    The project to modify.
.Parameter Configuration
    The configuration to modify (e.g. Debug or Release).
.Parameter Platform
    The platform configuration to modify (e.g. AnyCPU or x86)
.Example
    Set-FxCopWarningsAs Warnings

    Sets FxCop errors to be treated as warnings for all configurations of the active project.

.Example
    Set-FxCopWarningsAs Warnings $Project -Configuration Debug

    Sets FxCop errors to be treated as warnings for all Debug configurations of the given project.
#>
function Set-FxCopWarningsAs {
    param (
        [Parameter(Mandatory=$true)]
        [ValidateSet('Errors', 'Warnings')] [string] $TreatWarningsAs,
        $Project,
        [string] $Configuration,
        [string] $Platform,
        [switch] $Quiet
    )

    # make sure that we always have an msbuild project
    $Project = Get-MsBuildProject $project

    if (!$Quiet) {
        Write-Host "FxCop Errors are now $TreatWarningsAs in $($Project.FullPath)"
    }

    if ($TreatWarningsAs -eq 'Errors') {
        $TreatWarningsAsErrors = $true
    }
    else {
        $TreatWarningsAsErrors = $false
    }

    Set-MsBuildConfigurationProperty -Project $Project `
        -Name "CodeAnalysisTreatWarningsAsErrors" -Value $TreatWarningsAsErrors `
        -Configuration $Configuration -Platform $Platform
}

Export-ModuleMember Install-FxCop, Uninstall-FxCop, Enable-FxCop, Disable-FxCop, Set-FxCopWarningsAs
