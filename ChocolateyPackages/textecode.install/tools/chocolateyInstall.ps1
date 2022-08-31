Try { Set-ExecutionPolicy -ExecutionPolicy 'ByPass' -Scope 'Process' -Force -ErrorAction 'Stop' } Catch {}
$useAppx = ([Environment]::OSVersion.Version.Major -gt 10) -or (([Environment]::OSVersion.Version.Major -eq 10) -and ([Environment]::OSVersion.Version.Build -ge 14393))
$toolsDir = Split-Path -parent $MyInvocation.MyCommand.Definition
$version = '0.0.0'
$appxUrl = "https://github.com/OpenEpl/TextECode/releases/download/v${version}/TextECode_${version}_Appx.zip"
$portableUrl_x86 = "https://github.com/OpenEpl/TextECode/releases/download/v${version}/TextECode_${version}_Portable_x86.zip"
$portableUrl_x64 = "https://github.com/OpenEpl/TextECode/releases/download/v${version}/TextECode_${version}_Portable_x64.zip"
# sha512
$checksumAppx = ''
$checksumPortable_x86 = ''
$checksumPortable_x64 = ''
if ($useAppx) {
    $appxDir = Join-Path -Path $toolsDir -ChildPath "Appx"
    if ($env:ChocolateyForce) {
        Get-AppxPackage -Name "b74b3903-090f-4c4c-b09e-ae5a854d3990" -Publisher "CN=OpenEpl" | Remove-AppxPackage
    }
    Install-ChocolateyZipPackage -PackageName 'TextECodeAppx' `
        -Url $appxUrl -Checksum $checksumAppx -ChecksumType sha512 `
        -UnzipLocation $appxDir 
  (Get-ChildItem -File -Recurse -Path $appxDir -Include 'Install.ps1') | ForEach-Object { Invoke-Expression ("& `"" + $_.FullName + "`" -Force") }
    Remove-Item -Recurse -Force $appxDir
}
else {
    Install-ChocolateyZipPackage -PackageName 'TextECodePortable' `
        -Url $portableUrl_x86 -Checksum $checksumPortable_x86 -ChecksumType sha512 `
        -Url64 $portableUrl_x64 -Checksum64 $checksumPortable_x64 -ChecksumType sha512 `
        -UnzipLocation $toolsDir
  (Get-ChildItem -File -Recurse -Path $toolsDir -Include 'TextECode.exe') | Select-Object -First 1 | ForEach-Object { Install-BinFile 'TextECode' -Path $_.FullName }
}
