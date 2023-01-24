#######################################
## Globals
#######################################
# colors
$clrProjStart="`e[38;5;39m"
$clrCmdlets="`e[38;5;148m"
$clrTypesFmt="`e[38;5;215m"
$clrDefault="`e[39m"

#######################################
## Functions
#######################################

function findCmdletsInProject([string]$path) {
    Write-Information "`r`n${clrProjStart}==> Project $(Split-Path $path -Leaf) :$clrDefault" -InformationAction Continue
    $hasInfo = $false
    $cmdlets = @{}
    $files = Get-ChildItem $path -Recurse -Filter *.cs
    # find the verbs and nouns
    $verbs = getConstants ($files | Where-Object {$_.Name -match "\w+Verbs\w*\.cs"} | Select-Object -First 1)
    $nouns = getConstants ($files | Where-Object {$_.Name -match "\w+Nouns\w*\.cs"} | Select-Object -First 1)
    
    $files | ForEach-Object {findCmdletInFile $_.FullName $verbs $nouns $cmdlets}
    $types = (Test-Path $path\TypeData) ? (Get-ChildItem $path\TypeData -Filter *.Type.ps1xml) : $null
    $formats = (Test-Path $path\TypeData) ? (Get-ChildItem $path\FormatData -Filter *.Format.ps1xml) : $null
    $funcs = (getPSFunctions (Get-ChildItem $path -Attributes !D -Filter *.psm1 | Select-Object -First 1))
    
    if ($cmdlets.Count) {
        Write-Information "${clrCmdlets}----- CmdletsToExport:$clrDefault" -InformationAction Continue
        Write-Information "${clrCmdlets}  $(($cmdlets.Keys | ForEach-Object {"'$_'"}) -join `",`r`n`  ")$clrDefault" -InformationAction Continue
        $hasInfo = $true
    }
    if ($types) {
        Write-Information "`r`n${clrTypesFmt}----- TypesToProcess:$clrDefault" -InformationAction Continue
        Write-Information "${clrTypesFmt}  $(((quoteProperty $types Name) -replace "^'","'TypeData\") -join `",`r`n`  ")$clrDefault" -InformationAction Continue
        $hasInfo = $true
    }
    if ($formats) {
        Write-Information "`r`n${clrTypesFmt}----- FormatsToProcess:$clrDefault" -InformationAction Continue
        Write-Information "${clrTypesFmt}  $(((quoteProperty $formats Name) -replace "^'","'FormatData\") -join `",`r`n`  ")$clrDefault" -InformationAction Continue
        $hasInfo = $true
    }
    if ($funcs) {
        Write-Information "`r`n${clrTypesFmt}----- FunctionsToExport:$clrDefault" -InformationAction Continue
        Write-Information "${clrTypesFmt}  $((quoteProperty $funcs) -join `",`r`n`  ")$clrDefault" -InformationAction Continue
        $hasInfo = $true
    }
    if ($cmdlets.Count) {
        Write-Information "`r`n${clrCmdlets}----- Cmdlets Descriptions:$clrDefault" -InformationAction Continue
        foreach ($cmd in $cmdlets.GetEnumerator()) {
            Write-Information "${clrCmdlets}  $($cmd.Key)$clrDefault" -InformationAction Continue
            Write-Information "${clrCmdlets}  $($cmd.Value)$clrDefault" -InformationAction Continue
            Write-Information "" -InformationAction Continue
        }
    }

    if (Test-Path $path/Modules -PathType Container) {
        # we have submodules
        Get-ChildItem $path/Modules -Attributes D | ForEach-Object {
            $modFuncs = (getPSFunctions (Get-ChildItem $_ -Attributes !D -Filter *.psm1 | Select-Object -First 1))
            if ($modFuncs) {
                Write-Information "${clrProjStart}    --SubModule $($_.Name)$clrDefault" -InformationAction Continue
                Write-Information "${clrTypesFmt}    ----- FunctionsToExport:$clrDefault" -InformationAction Continue
                Write-Information "${clrTypesFmt}  $((quoteProperty $modFuncs) -join `",`r`n`  ")$clrDefault" -InformationAction Continue
                Write-Information "`r`n" -InformationAction Continue
            }
        }
    }
    
    if ($hasInfo) {
        Write-Information "${clrProjStart}==========Complete$clrDefault" -InformationAction Continue
    } else {
        Write-Information "${clrProjStart}---Nothing to report$clrDefault" -InformationAction Continue
    }
}

function findCmdletInFile([string]$file, [hashtable]$verbs, [hashtable]$nouns, [hashtable]$result) {
    $content = gc $file
    $start=$false
    $done=$false
    $cmdletName = $null
    $foundNoDesc = $false
    foreach ($line in $content) {
        if ($line -match '\[Cmdlet\(\w*Verbs\w*\.(\w+),\s*\w*Nouns\w*\.(\w+)') {
            $verb = $verbs.ContainsKey($Matches[1]) ? $verbs[$Matches[1]] : $Matches[1]
            $noun = $nouns.ContainsKey($Matches[2]) ? $nouns[$Matches[2]] : $Matches[2]
            $cmdletName = "$verb-$noun"
            $start = $true
            $foundNoDesc = $true
        }
        if ($start -and !$done) {
            if ($line -match 'Description\("(.+)"\)') {
                if ($cmdletName) {
                    $result[$cmdletName] = $Matches[1]
                    $foundNoDesc = $false
                } else {
                    Write-Warning "Found description in $file before the cmdlet name"
                }
                $done = $true
            }
        }
        if ($done) {
            break
        }
    }
    if ($cmdletName -and $foundNoDesc) {
        Write-Warning "Cmdlet $cmdletName [$(Split-Path $file -Leaf)] has no description"
    }
}

function getConstants([System.IO.FileSystemInfo]$file) {
    [hashtable]$result = @{}
    if ($file) {
        Get-Content $file | ForEach-Object {
            if ($_ -match '\s+public const string (\w+)\s*=\s*"(\w+)";') {
                $result[$Matches[1]] = $Matches[2]
            }
        }
    }
    return $result;
}

function getPSFunctions([System.IO.FileSystemInfo]$file) {
    $func = @()
    if ($file) {
        Get-Content $file | ForEach-Object {
            if ($_ -match '^\s*function\s([\w-_]+)\b') {
                $func += $Matches[1]
            }
        }
    }
    return $func
}

function quoteProperty($col, $propName=$null) {
    $adjCol = $propName ? ($col | Select-Object -ExpandProperty $propName) : $col
    return $adjCol | ForEach-Object {"'$_'"}
}

#######################################
## Main
#######################################

pushd $PSScriptRoot
pushd ../Src

ls . -Attributes D | ForEach-Object {findCmdletsInProject $_.FullName}

popd
popd