param($installPath, $toolsPath, $package, $project)

# if there isn't a project file, there is nothing to do
if (!$project) { return }

Import-Module (Join-Path $toolsPath "FxCop.psm1") -Force

# turn it on
Install-FxCop $project
$project.Save()