param ($Version)
$packageDir = Split-Path -parent $MyInvocation.MyCommand.Definition
$nuspecPath = Join-Path -Path $packageDir -ChildPath 'textecode.nuspec'

[xml]$nuspec = Get-Content $nuspecPath
$nuspec.package.metadata.version = $Version
$nuspec.package.metadata.dependencies.dependency.version = "[$Version]"
$nuspec.save($nuspecPath)
