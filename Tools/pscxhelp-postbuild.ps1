[CmdletBinding()]
param([string]$outDir, [string]$configuration)

# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

Write-Host "In $PWD running script $($MyInvocation.MyCommand) from $PSScriptRoot with params outdir as $outDir and config $configuration"

$solDir = $PWD
$packDir = (Get-ChildItem (Join-Path $solDir "..\Output\Pscx") -Attributes D)[0]
$version = (Import-PowerShellDataFile "$outDir\Pscx.psd1").ModuleVersion
$outputPath = Join-Path $outDir "Output"

# create help files
Write-Host "Create help files..."
pushd $outDir
# copy the Apps folder from PSCX - not automatically copied by the build system
Copy-Item $solDir\Pscx\bin\$configuration\net6.0\Apps . -Recurse -Force
Import-Module .\Pscx.psd1
Import-Module .\PscxHelp.psd1

rm -Recurse -Force $outputPath -ErrorAction Ignore
mkdir $outputPath -Force

.\Scripts\GeneratePscxHelpXml.ps1 $outputPath .\Help -Configuration $configuration
.\Scripts\GenerateAboutPsxcHelpTxt.ps1 $outputPath -Configuration $configuration

popd

# copy help files
Write-Host "Copy help files... (to $packDir)"
gci $outputPath -Exclude Merged* | foreach {cp $_ $packDir}

# package the PSCX module as zip
Write-Host "Package PSCX module..."
pushd ..\Output
Compress-Archive -Path .\Pscx -DestinationPath .\Pscx-$version.zip
#rm .\Pscx -Recurse -Force
popd
