function findCmdletsInProject([string]$path) {
    Write-Information "Project $(Split-Path $path -Leaf) :" -InformationAction Continue
    $result = @{}
    $files = ls $path -Recurse -Filter *.cs
    $files | ForEach-Object {findCmdletInFile $_.FullName $result}
    return $result
}

function findCmdletInFile([string]$file, [hashtable]$result) {
    $content = gc $file
    $start=$false
    $done=$false
    $cmdletName = $null
    foreach ($line in $content) {
        if ($line -match '\[Cmdlet\(\w*Verbs\w*\.(\w+),\s+\w*Nouns\w*\.(\w+)') {
            $cmdletName = "$($Matches[1])-$($Matches[2])"
            $start = $true
        }
        if ($start -and !$done) {
            if ($line -match 'Description\("(.+)"\)') {
                if ($cmdletName) {
                    $result[$cmdletName] = $Matches[1]
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
}


pushd $PSScriptRoot
pushd ../Src

ls . -Attributes D | ForEach-Object {findCmdletsInProject $_.FullName}

popd
popd