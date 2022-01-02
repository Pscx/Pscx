[CmdletBinding()]
param (
    [Parameter(ParameterSetName="Publish")]
    [switch]
    $Publish,

    [Parameter(ParameterSetName="Publish")]
    [string]
    $NuGetApiKey
)
$ErrorActionPreference = 'Stop'

$manifest = Invoke-Expression (Get-Content $PSScriptRoot\src\Pscx\Pscx.psd1 -Raw)
$version = $manifest.ModuleVersion

"Version is: $version, prerelease is: $($manifest.PrivateData.PSData.Prerelease)"

$modulePath = Join-Path (Split-Path $profile.CurrentUserAllHosts -Parent) Modules\Pscx\$version
if (Test-Path -LiteralPath $modulePath) {
    "Removing dir: $modulePath"
    Remove-Item $modulePath -Recurse -Force
}

$outPath = Join-Path $PSScriptRoot src\Pscx\bin\Release -Resolve

& {
    $VerbosePreference = 'Continue'
    "Copying output from $outPath to $modulePath"
    Copy-Item $outPath $modulePath -Recurse

    Remove-Item $modulePath\*.pdb
    Remove-Item $modulePath\PowerCollections.xml
    Remove-Item $modulePath\SoftUni.Wintellect.PowerCollections.xml
    Remove-Item $modulePath\Trinet.Core.IO.Ntfs.xml

    Remove-Item $modulePath\x86 -Recurse -Force -ErrorAction Ignore
    Remove-Item $modulePath\x64 -Recurse -Force -ErrorAction Ignore
}

if ($Publish) {
    Publish-Module -Name Pscx -AllowPrerelease -NuGetApiKey $NuGetApiKey
}
