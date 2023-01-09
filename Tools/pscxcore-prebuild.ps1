#######################################
## Globals                           ##
#######################################
[CmdletBinding()]
param([string]$outDir, [string]$configuration)

# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

Write-Host "In $PWD running script $($MyInvocation.MyCommand) from $PSScriptRoot with params outdir as $outDir and config $configuration"

$solDir = $PWD

# cleanup the packing folder
pushd ..\Output
rm * -Recurse -Force
popd
