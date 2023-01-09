#######################################
## Globals                           ##
#######################################
[CmdletBinding()]
param([string]$outDir, [string]$configuration)

# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

Write-Host "In $PWD running script $($MyInvocation.MyCommand) from $PSScriptRoot with params outdir as $outDir and config $configuration"

$solDir = $PWD
$pscxDll = Join-Path $outDir "Pscx.Core.dll"
$signTool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\signtool.exe"

#sign if enabled
if ($configuration -eq "Release-Signed") {
    Write-host "$signTool sign /t http://timestamp.digicert.com /sha1 BB25149CDAF879A29DB6A011F6FC874AF32CBF51 $pscxDll"
    & $signTool sign /t http://timestamp.digicert.com /sha1 BB25149CDAF879A29DB6A011F6FC874AF32CBF51 "$pscxDll"
    Write-Host ".\SignScripts.ps1" 
    .\SignScripts.ps1
}

