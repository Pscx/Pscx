#---------------------------------------------------------
### <Script>
### <Author>Keith Hill</Author>
### <Description>
### This script generates the about_Pscx.help.txt file
### </Description>
### </Script>
#---------------------------------------------------------
param([string]$outputDir = $(throw "You must specify the output path to emit the generated file"),
      [string]$configuration = $(throw "You must specify the build configuration"))

# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

function WrapText($width = 74, $indent = 8) {

    begin {
        $indent = " " * $indent
        $whitespace = "`t`r`n ".ToCharArray()
        [Text.StringBuilder]$output = $indent
        $currentLineWidth = 0
    }

    process {
        if (!$_) {
            return
        }
        
        $words = $_.Split($whitespace, [StringSplitOptions]::RemoveEmptyEntries)
        
        $index = 0
        $count = 0
        
        while(($index + $count) -lt $words.Length) {
        
            while($currentLineWidth -le $width) {
                $w = $words[$index + $count].Length + 1;
                
                if(($currentLineWidth + $w) -gt $width) {
                    break
                }
                
                $currentLineWidth += $w
                $count++
            }
            
            if($count -gt 0) {
                $words[$index..($index + $count - 1)] |% {
                    $output.Append($_)  | out-null
                    $output.Append(' ') | out-null
                }
                
                $output.AppendLine() | out-null
                $output.Append($indent) | out-null
            }
            else {
                $word = $words[$index]
                $line = $null;
                
                do {
                    if($word.Length -gt $width) {
                        $line = $word.Substring(0, $width)
                        $word = $word.Substring($width)
                    }
                    else {
                        $line = $word;
                        $word = $null;
                    }
                    
                    $output.AppendLine($indent + $line) | out-null
                } while($word)
                
                $count++
            }
        
            $index += $count

            $count = 0
            $currentLineWidth = 0
        }
    }

    end {
        $output.ToString()
    }
}

function WriteLine {
    "" | OutAboutHelpFile
}

filter OutAboutHelpFile {
    $_ | Out-File $AboutPscxHelpPath -Encoding Utf8 -Append
}

#---------------------------------------------------------
# Script main
#---------------------------------------------------------
$ModuleDir         = Join-Path $PSScriptRoot "..\"
#$PscxPath          = Join-Path $ModuleDir "Pscx"            
#$PscxManifest      = "$PscxPath.psd1"            
#$PscxModule        = Join-Path $ModuleDir "Pscx.dll"             
$outputDir         = Resolve-Path $outputDir
$templateDir       = Split-Path $outputDir -parent
$providerHelpPath  = Split-Path $outputDir -parent
$AboutPscxHelpPath = Join-Path $outputDir about_Pscx.help.txt
$MergedHelpPath    = Join-path $outputDir MergedHelp-Pscx.xml

#Import-Module $PscxManifest
$PscxModule = Get-Module Pscx

#---------------------------------------------------------
# Process the Module functions
#---------------------------------------------------------
$functions = Get-Command -type function -Module pscx | Foreach {Get-Help $_.Name} | Sort Name

#---------------------------------------------------------
# Insert Header template
#---------------------------------------------------------
New-Item $AboutPscxHelpPath -Type File -Force > $null
Get-Content (join-path $templateDir about_Pscx_header.txt) |
    % { $_ -replace '<VERSION_NUMBER>', $PscxModule.Version } | OutAboutHelpFile
    
#---------------------------------------------------------
# Process Cmdlets
#---------------------------------------------------------
WriteLine
"CMDLETS" | OutAboutHelpFile
"    To see what cmdlets are provided by PSCX, execute the command:" | OutAboutHelpFile
"    Get-Command -Module Pscx* -CommandType Cmdlet" | OutAboutHelpFile
"    The current PSCX cmdlets are listed below:" | OutAboutHelpFile
WriteLine
$MergedXml = [Xml](Get-Content $MergedHelpPath)
$MergedXml.Cmdlets.Cmdlet | Sort Noun, Verb | % {
    "    $($_.Verb)-$($_.Noun)"
    $_.DetailedDescription.Trim() | WrapText
} | OutAboutHelpFile

$MergedHelpPath = Join-path $outputDir MergedHelp-Pscx.Win.xml
if (Test-Path $MergedHelpPath -PathType Leaf) {
    $MergedXml = [Xml](Get-Content $MergedHelpPath)
    $MergedXml.Cmdlets.Cmdlet | Sort Noun, Verb | % {
        "    $($_.Verb)-$($_.Noun)"
        $_.DetailedDescription.Trim() | WrapText
    } | OutAboutHelpFile
}

#---------------------------------------------------------
# Process Providers
#---------------------------------------------------------
WriteLine
"PROVIDERS" | OutAboutHelpFile
gci $providerHelpPath\Provider*.xml | ? {$_.Name -notmatch 'Provider_template'} | Sort Name  | % {
    $provider = [xml](Get-Content $_)
    "    $($provider.providerHelp.Name.Trim())"
    $provider.providerHelp.Synopsis.Trim() | WrapText
} | OutAboutHelpFile

#---------------------------------------------------------
# Process Functions
#---------------------------------------------------------
"FUNCTIONS" | OutAboutHelpFile
"    To see what functions are provided by PSCX, execute the command:" | OutAboutHelpFile
"    Get-Command -Module Pscx* -CommandType Function" | OutAboutHelpFile
"    The current PSCX functions are listed below:" | OutAboutHelpFile
WriteLine
foreach ($function in $functions) {
    $aliases = Get-Alias | ?{$_.Definition -match "^$($function.Name)`$"} | sort Name | %{$_.Name}
    if ($aliases) {
        $OFS = ', '
        "    $($function.name) ($aliases)" | OutAboutHelpFile
    }
    else {
        "    $($function.name)" | OutAboutHelpFile
    }
    $function.Synopsis.Trim() | WrapText | OutAboutHelpFile
}

#---------------------------------------------------------
# Process Aliases
#---------------------------------------------------------
"PSCX ALIASES" | OutAboutHelpFile
"    To see what aliases get created by PSCX, execute the command:" | OutAboutHelpFile
"    Get-Command -Module Pscx* -CommandType Alias" | OutAboutHelpFile
"    The current PSCX defined aliases are listed below:" | OutAboutHelpFile
WriteLine
$pscxAliases = Get-Command -type Alias -module Pscx | Sort Name
$longestAlias = ($pscxAliases | %{$_.Name.Length} | Measure-object -max).Maximum
foreach ($alias in $pscxAliases) {
    Write-Host "Processing $alias"
    $len = $alias.Name.Length
    $diff = $longestAlias - $len
    "    $($alias.Name)" + " " * $diff + 
        " : alias for $($alias.Definition) $(get-command $alias.Definition | %{$_.CommandType.ToString().ToLower()})" | 
        OutAboutHelpFile
}
WriteLine

#---------------------------------------------------------
# Append footer template
#---------------------------------------------------------
Get-Content (join-path $templateDir about_Pscx_footer.txt) | OutAboutHelpFile
