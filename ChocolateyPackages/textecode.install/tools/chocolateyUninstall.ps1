$ErrorActionPreference = 'Stop'
$useAppx = ([Environment]::OSVersion.Version.Major -gt 10) -or (([Environment]::OSVersion.Version.Major -eq 10) -and ([Environment]::OSVersion.Version.Build -ge 14393))
if ($useAppx) {
    Get-AppxPackage -Name "b74b3903-090f-4c4c-b09e-ae5a854d3990" -Publisher "CN=OpenEpl" | Remove-AppxPackage
}
