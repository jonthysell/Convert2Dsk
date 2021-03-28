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

try
{
    dotnet msbuild $BuildArgs.Split() -restore -p:Configuration=Release -p:TrimMode=link -p:PublishDir="$RepoRoot\$OutputRoot\$TargetOutputDirectory" "$SolutionPath"
    if (!$?) {
    	throw 'Build failed!'
    }

    Copy-Item "README.md" -Destination "$OutputRoot\$TargetOutputDirectory\ReadMe.txt"
    Copy-Item "LICENSE.md" -Destination "$OutputRoot\$TargetOutputDirectory\License.txt"
    Copy-Item "CHANGELOG.md" -Destination "$OutputRoot\$TargetOutputDirectory\ChangeLog.txt"
}
finally
{
    Set-Location -Path "$StartingLocation"
}