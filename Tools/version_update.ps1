#######################################
## Globals
#######################################
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true, Position=1)]
    [string]$newVersion
)

$pscxAssemblyInfo = "Pscx.Core/Properties/PscxAssemblyInfo.cs"

#######################################
## Functions
#######################################
class AssemblyInfo {
    [string]$version
    [string]$authors
    [string]$copyright
    [string]$product
    [string]$description
}

function updateVersionPSData([string]$filePath, [string]$version) {
    $rawContent = Get-Content -Path $filePath -Raw -Encoding utf8
    $fileModified = $false
    $contentLines = $rawContent -split '[\r\n]+'
    foreach ($line in $contentLines) {
        # includes the quotes in the match group - such that it covers both single, double quotes
        if ($line -match '\s*ModuleVersion\s*=\s*[''"](.+)[''"]$') {
            $oldVer = $Matches[1]
            if ($oldVer -ne $version) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldVer, "$version"))
                $fileModified = $true
            }
            break
        }
    }
    if ($fileModified) {
        Set-Content -Path $filePath -Value $rawContent -NoNewline -Encoding utf8
        Write-Output "`e[38;5;220m>> Updated module $((Get-Item $filePath).BaseName) version to: $version`e[39;49m"
    }
}

function updateVersionAssembly([string]$filePath, [string]$version) {
    $rawContent = Get-Content -Path $filePath -Raw -Encoding utf8
    $fileModified = $false
    $contentLines = $rawContent -split '[\r\n]+'
    foreach ($line in $contentLines) {
        # covers both AssemblyVersion and AssemblyFileVersion
        if ($line -match '\[assembly:\s+Assembly\w*Version\("(.+)"\)\]') {
            $oldVer = $Matches[1]
            if ($oldVer -ne $version) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldVer, $version))
                $fileModified = $true
            }
        } elseif ($line -match 'const string (Assembly|File)Version\s*=\s*"(.+)";') {
            $oldVer = $Matches[2]
            if ($oldVer -ne $version) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldVer, $version))
                $fileModified = $true
            }
        }
    }    
    if ($fileModified) {
        Set-Content -Path $filePath -Value $rawContent -NoNewline -Encoding utf8
        Write-Output "`e[38;5;220m>> Updated assembly file $((Get-Item $filePath).BaseName) version to: $version`e[39;49m"
    }
}

function updateVSProjectFile([string]$filePath, [AssemblyInfo]$asi) {
    $rawContent = Get-Content -Path $filePath -Raw -Encoding utf8
    $fileModified = 0
    $contentLines = $rawContent -split '[\r\n]+'
    foreach ($line in $contentLines) {
        # all other versions are assumed to leverage the $AssemblyVersion property
        if ($line -match '<AssemblyVersion>(.+)</AssemblyVersion>') {
            $oldVer = $Matches[1]
            if ($oldVer -ne $asi.version) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldVer, $asi.version))
                $fileModified++
            }
        } elseif ($line -match '<Copyright>(.+)</Copyright>') {
            $oldCpy = $Matches[1]
            if ($oldCpy -ne $asi.copyright) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldCpy, $asi.copyright))
                $fileModified++
            }
        } elseif ($line -match '<Authors>(.+)</Authors>') {
            $oldAuth = $Matches[1]
            if ($oldAuth -ne $asi.authors) {
                $rawContent = $rawContent.Replace($line, $line.Replace($oldAuth, $asi.authors))
                $fileModified++
            }        
        }
        if ($fileModified -gt 2) {
            break
        }
    }    
    if ($fileModified -gt 0) {
        Set-Content -Path $filePath -Value $rawContent -NoNewline -Encoding utf8
        Write-Output "`e[38;5;220m>> Updated C# project file $((Get-Item $filePath).BaseName) version to: $($asi.version)`e[39;49m"
    }
}

function updateSeedAssemblyInfo([string]$filePath, [string]$newVer) {
    $pscxInfo = Get-Content -Path $filePath -Encoding utf8 -Raw
    $asmInfo = [AssemblyInfo]::new()
    $asmInfo.version = $newVer
    $curYear = (Get-Date).Year
    $fileModified = $false

    $pscxInfoLines = $pscxInfo -split '[\r\n]+'
    foreach ($ln in $pscxInfoLines) {
        if ($ln -match '\sCopyright\s*=\s*"(.+)";') {
            $asmInfo.copyright = $Matches[1].Replace('\xa9', "©")
            # update the current copyright year
            if ($asmInfo.copyright -match '\s\d{4}-(\d{4})\s') {
                $cpyYear = $Matches[1]
                if ($cpyYear -ne $curYear) {
                    $asmInfo.copyright = $asmInfo.copyright.Replace($cpyYear, $curYear)
                    $pscxInfo = $pscxInfo.Replace($ln, $ln.Replace($cpyYear, $curYear))
                    $fileModified = $true
                }
            }
        } elseif ($ln -match 'const string Company\s*=\s*"(.+)";') {
            $asmInfo.authors = $Matches[1]
        } elseif ($ln -match 'const string (Assembly|File)Version\s*=\s*"(.+)";') {
            $oldVer = $Matches[2]
            if ($oldVer -ne $newVer) {
                $pscxInfo = $pscxInfo.Replace($ln, $ln.Replace($oldVer, $newVer))
                $fileModified = $true
            }
        } elseif ($ln -match 'const string Product\s*=\s*"(.+)";') {
            $asmInfo.product = $Matches[1]
        } elseif ($ln -match 'const string Description\s*=\s*"(.+)";') {
            $asmInfo.description = $Matches[1]
        }
    }
    if ($fileModified) {
        Set-Content -Path $filePath -Value $pscxInfo -NoNewline -Encoding utf8
        Write-Information "`e[38;5;220m>> Updated seed assembly file $((Get-Item $filePath).BaseName) version to: $newVer`e[39;49m" -InformationAction Continue
    }
    return $asmInfo
}

#######################################
## Main
#######################################
Push-Location $PSScriptRoot

Push-Location ..\Src

[AssemblyInfo]$asmbInfo = updateSeedAssemblyInfo ((Get-Item $pscxAssemblyInfo).FullName) $newVersion

Get-ChildItem -Path * -Recurse -Include *.psd1,*.csproj,AssemblyInfo*.cs | ForEach-Object {
    $f = $_
    switch ($f.Extension) {
        ".psd1" { 
            updateVersionPSData $f.FullName $newVersion
            break
         }
        ".cs" {
            updateVersionAssembly $f.FullName $newVersion
            break
         }
        ".csproj" {
            updateVSProjectFile $f.FullName $asmbInfo
            break
        }
    }
}

Pop-Location

Pop-Location

Write-Output "`e[38;5;147m>>> Done`e[39;49m"