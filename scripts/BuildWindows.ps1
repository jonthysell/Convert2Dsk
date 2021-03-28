param()

[string] $Product = "Convert2Dsk"
[string] $Target = "Windows"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=win-x86 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeAllContentForSelfExtract=true"

& "$PSScriptRoot\ZipRelease.ps1" -Product $Product -Target $Target
