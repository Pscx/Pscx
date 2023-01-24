#---------------------------------------------------------
# Desc: This script generates the Pscx.dll-Help.xml file
#---------------------------------------------------------
param([string]$outputDir = $(throw "You must specify the output path to emit the generated file"),
      [string]$localizedHelpPath = $(throw "You must specify the path to the localized help dir"),
      [string]$configuration = $(throw "You must specify the build configuration"))
      
# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

$ModuleDir        = Join-Path $PSScriptRoot "..\"
#$PscxManifest     = "$PscxPath.psd1"            
$PscxModule       = Join-Path $ModuleDir "Pscx.dll"             
$outputDir        = Resolve-Path $outputDir
$ProviderHelpPath = Split-Path $outputDir -parent
$transformsDir    = Join-Path $ProviderHelpPath Transformations


# Import-Module $PscxManifest

# Test the XML help files
gci $localizedHelpPath\*.xml  | Foreach {
    if (!(Test-Xml $_)) {
        Test-Xml $_ -verbose
        Write-Error "$_ is not a valid XML file"
        exit 1
    }
}
gci $providerHelpPath\Provider*.xml  | Foreach {
    if (!(Test-Xml $_)) {
        Test-Xml $_ -verbose
        Write-Error "$_ is not a valid XML file"
        exit 1
    }
}

function buildAssemblyHelp([string]$moduleDll, [string]$outDir, [string]$localizedHelpDir) {
    Write-Host "Building Help for $moduleDll"

    $MergedHelpPath   = Join-Path $outDir "MergedHelp-$([System.IO.Path]::GetFileNameWithoutExtension($moduleDll)).xml"
    $PscxHelpPath     = Join-Path $outDir "$(Split-Path $moduleDll -Leaf)-Help.xml"

    Get-PSSnapinHelp $moduleDll -LocalizedHelpPath $localizedHelpDir -OutputPath $MergedHelpPath

$contents = Get-Content $MergedHelpPath
$contents | foreach {$_ -replace 'PscxPathInfo','String'} | Out-File $MergedHelpPath -Encoding Utf8

Convert-Xml $MergedHelpPath -xslt $transformsDir\Maml.xslt -OutputPath $PscxHelpPath

$helpXml = [xml](Get-Content $PscxHelpPath)
$attrs = $helpXml.helpItems.Attributes | Where Name -ne schema
$attrs | Foreach {[void]$helpXml.helpItems.Attributes.Remove($_)}

$msHelpAttr = $helpXml.CreateAttribute('xmlns', 'MSHelp', 'http://www.w3.org/2000/xmlns/')
$msHelpAttr.Value = 'http://msdn.microsoft.com/mshelp'
$attrs += $msHelpAttr

$helpXml.helpItems.command | %{$cmd = $_; $attrs | % {[void]$cmd.SetAttributeNode($_.Clone())}}
$helpXml.Save($PscxHelpPath)

# Low tech approach to merging in the provider help
$helpfile = Get-Content $PscxHelpPath | ? {$_ -notmatch '</helpItems>'}
$providerHelp = @()
gci $providerHelpPath\Provider*.xml | ? {$_.Name -notmatch 'Provider_template'} | Foreach {
    Write-Host "Processing $_"
    $providerHelp += Get-Content $_
}

$helpfile += $providerHelp
$helpfile += '</helpItems>'
$helpfile | Out-File $PscxHelpPath -Encoding Utf8
}

# Pscx.dll help
buildAssemblyHelp $PscxModule $outputDir $localizedHelpPath

# PscxWin help
$PscxModule = Join-Path $ModuleDir "Pscx.Win.dll"
if (Test-Path $PscxModule -PathType Leaf) {
    buildAssemblyHelp $PscxModule $outputDir $localizedHelpPath
}

