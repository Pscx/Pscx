#---------------------------------------------------------
# Desc: This script generates the Pscx.dll-Help.xml file
#---------------------------------------------------------
param([string]$outputDir = $(throw "You must specify the output path to emit the generated file"),
      [string]$localizedHelpPath = $(throw "You must specify the path to the localized help dir"),
      [string]$configuration = $(throw "You must specify the build configuration"))
      
$ScriptDir        = Split-Path $MyInvocation.MyCommand.Path -Parent
$ModuleDir        = "$ScriptDir\..\..\Pscx\bin\$configuration"      
$PscxPath         = Join-Path $ModuleDir "Pscx"            
$PscxManifest     = "$PscxPath.psd1"            
$PscxModule       = "$PscxPath.dll"             
$outputDir        = Resolve-Path $outputDir
$ProviderHelpPath = Split-Path $outputDir -parent
$transformsDir    = Join-Path $ProviderHelpPath Transformations
$MergedHelpPath   = Join-Path $outputDir MergedHelp.xml
$PscxHelpPath     = Join-Path $outputDir Pscx.dll-Help.xml

Import-Module $PscxManifest

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

Get-PSSnapinHelp $PscxModule -LocalizedHelpPath $localizedHelpPath -OutputPath $MergedHelpPath

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
