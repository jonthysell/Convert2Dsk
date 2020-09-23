[string] $Product = "Convert2Dsk"
[string] $Target = "MacOS"

& "$PSScriptRoot\Build.ps1" -Product $Product -Target $Target -BuildArgs "-target:Publish -p:RuntimeIdentifier=osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true"

& "$PSScriptRoot\TarRelease.ps1" -Product $Product -Target $Target
