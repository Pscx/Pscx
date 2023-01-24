#######################################
## Globals                           ##
#######################################
[CmdletBinding()]
param([string]$outDir, [string]$configuration)

# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

Write-Host "In $PWD running script $($MyInvocation.MyCommand) from $PSScriptRoot with params outdir as $outDir and config $configuration"

$solDir = $PWD
$pscxDll = Join-Path $outDir "Pscx.dll"
$signTool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\signtool.exe"
$version = (Import-PowerShellDataFile (Join-Path $outDir "Pscx.psd1")).ModuleVersion

# copy release notes
cp ..\CHANGELOG.md $outDir

# copy the Apps
pushd $outDir
if (!(Test-Path "Apps" -PathType Container)) {
    mkdir "Apps\Win"
    mkdir "Apps\macOS"
    mkdir "Apps\Linux"
}
cp $solDir\..\Imports\Less-553\*.* .\Apps\Win\
cp $solDir\..\Imports\gsudo\win\gsudo.exe .\Apps\Win\sudo.exe
cp $solDir\..\Imports\7zip\win\x64\7z.* .\Apps\Win\
cp $solDir\..\Imports\7zip\macOS\7zz .\Apps\macOS\
cp $solDir\..\Imports\7zip\linux\x64\7zz .\Apps\Linux\
popd

#sign if enabled
if ($configuration -eq "Release-Signed") {
    Write-host "$signTool sign /t http://timestamp.digicert.com /sha1 BB25149CDAF879A29DB6A011F6FC874AF32CBF51 $pscxDll"
    & $signTool sign /t http://timestamp.digicert.com /sha1 BB25149CDAF879A29DB6A011F6FC874AF32CBF51 "$pscxDll"
    Write-Host ".\SignScripts.ps1" 
    .\SignScripts.ps1
}

#package the output - the packDir is created by the pscxwin-postbuild script that runs before this one
$packDir = Join-Path $solDir "..\Output\Pscx\$version\"
if (!(Test-Path $packDir)) {
    mkdir $packDir
}

pushd $outDir
cp Pscx.Core.dll,Pscx.dll,Pscx.psd1,Pscx.psm1,Pscx.UserPreferences.ps1,Pscx.ico $packDir -Force
cp $solDir\..\CHANGELOG.md $packDir -Force
cp Apps $packDir -Recurse -Force
cp FormatData $packDir -Recurse -Force
cp Modules $packDir -Recurse -Force
cp TypeData $packDir -Recurse -Force
popd

