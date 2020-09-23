param(
    [string]$Product,
    [string]$Target,
    [string]$BuildArgs = ""
)

[string] $SolutionPath = "src\$Product.sln"

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

[string] $OutputRoot = "bld"
[string] $TargetOutputDirectory = "$Product.$Target"

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

if (Test-Path "$OutputRoot\$TargetOutputDirectory") {
    Write-Host "Clean output folder..."
    Remove-Item "$OutputRoot\$TargetOutputDirectory" -Recurse
}

Write-Host "Build release..."

dotnet msbuild $BuildArgs.Split() -restore -p:Configuration=Release -p:PublishDir="$RepoRoot\$OutputRoot\$TargetOutputDirectory" "$SolutionPath"
Copy-Item "README.md" -Destination "$OutputRoot\$TargetOutputDirectory\ReadMe.txt"
Copy-Item "LICENSE.md" -Destination "$OutputRoot\$TargetOutputDirectory\License.txt"

Set-Location -Path "$StartingLocation"
