param ($Version, $ChecksumAppx, $ChecksumPortable_x86, $ChecksumPortable_x64)
$packageDir = Split-Path -parent $MyInvocation.MyCommand.Definition
$nuspecPath = Join-Path -Path $packageDir -ChildPath 'textecode.install.nuspec'
$chocolateyInstallPath = Join-Path -Path $packageDir -ChildPath 'tools\chocolateyInstall.ps1'

[xml]$nuspec = Get-Content $nuspecPath
$nuspec.package.metadata.version = $Version
$nuspec.save($nuspecPath)

(Get-Content -Raw $chocolateyInstallPath) `
    -Replace "([`$]version\s*=\s*)('.*')", "`$1'$($Version)'" `
    -Replace "([`$]checksumAppx\s*=\s*)('.*')", "`$1'$($ChecksumAppx)'" `
    -Replace "([`$]checksumPortable_x86\s*=\s*)('.*')", "`$1'$($ChecksumPortable_x86)'" `
    -Replace "([`$]checksumPortable_x64\s*=\s*)('.*')", "`$1'$($ChecksumPortable_x64)'" | Out-File $chocolateyInstallPath -Encoding UTF8 -NoNewline -Force