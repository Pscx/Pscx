<#
#>
function ScriptDiskPart($script)
{
    $file = [io.path]::GetTempFileName()

    try
    {
        Set-Content $file $script -Encoding utf8
        & diskpart /s $file > $null
    }
    finally
    {
        Remove-Item $file
    }
}

function EnsureFileExists($VHD)
{
    if (-not (test-path $VHD -PathType leaf))
    {
        throw "File $VHD not found."
    }
}

<#
.SYNOPSIS
    Mounts a Virtual Hard Drive (VHD) file.
.PARAMETER Path
    Path to the VHD file.
#>
function Mount-PscxVHD
{
    [CmdletBinding()]
    param
    (
        [Parameter(Position=1, Mandatory=$true)]
        $Path
    )

    EnsureFileExists $Path
    ScriptDiskPart @"
        select vdisk file="$Path"
        attach vdisk
"@

}

<#
.SYNOPSIS
    Dismounts a Virtual Hard Drive (VHD) file.
.PARAMETER Path
    Path to the VHD file.
#>
function Dismount-PscxVHD
{
    [CmdletBinding()]
    param
    (
        [Parameter(Position=1, Mandatory=$true)]
        $Path
    )

    EnsureFileExists $Path
    ScriptDiskPart @"
        select vdisk file="$Path"
        detach vdisk
"@
}

Export-ModuleMember Mount-PscxVHD, Dismount-PscxVHD
