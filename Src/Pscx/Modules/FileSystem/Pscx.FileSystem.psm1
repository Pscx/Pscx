Set-StrictMode -Version Latest

# Only prepend format data once - unloading a module doesn't undo the effects of prepending format data
if (!$Pscx:Session['FileSystem_PrependedFormatData'])
{
    $ScriptDir = Split-Path $MyInvocation.MyCommand.Path -Parent
    Write-Verbose "PSCX prepending format data $ScriptDir\Pscx.FileSystem.Format.ps1xml."
	if ($PSVersionTable.PSVersion.Major -le 4) {
		Update-FormatData -PrependPath "$ScriptDir\Pscx.FileSystem.Format.ps1xml"
	}
	else {
		Update-FormatData -PrependPath "$ScriptDir\Pscx.FileSystem.Format.5.0.ps1xml"
	}
    $Pscx:Session['FileSystem_PrependedFormatData'] = $true
}

<#
.SYNOPSIS
    Calculates the sizes of the specified directory and adds that size
    as a "Length" NoteProperty to the input DirectoryInfo object.
.DESCRIPTION
    Calculates the sizes of the specified directory and adds that size
    as a "Length" NoteProperty to the input DirectoryInfo object.  NOTE: Computing the
    size of a directory can noticeably impact performance. 
.PARAMETER InputObject
    The directory object (System.IO.DirectoryInfo) on which to add the Length property
.EXAMPLE
    C:\PS> Get-ChildItem . -Recurse | Add-DirectoryLength | Sort Length
    This example shows how you can compute the directory size for each directory passed via the pipeline
    and add that info to each DirectoryInfo object.
#>
function Add-DirectoryLength
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [AllowNull()]
        [PSObject]
        $InputObject
    )

    Begin 
    {
        function ProcessFile([string]$path) {
            (get-item -LiteralPath $path -Force).Length
        }
        
        function ProcessDirectory([string]$path) {
            $dirSize = 0
            $items = get-childitem -LiteralPath $path -Force -ea $ErrorActionPreference | sort @{e={$_.PSIsContainer}}
            if ($items -eq $null) {
                return $null
            }
            foreach ($item in $items) {
                if ($item.PSIsContainer) {
                    $dirSize += ProcessDirectory($item.FullName)
                }
                else {
                    $dirSize += ProcessFile($item.FullName)
                }
            }
            $dirSize
        }
    }

    Process {
        if ($InputObject -is [System.IO.DirectoryInfo]) {
            $dirSize = ProcessDirectory($InputObject.FullName)
            Add-Member NoteProperty Length $dirSize -InputObject $InputObject
        }
        $InputObject
    }
}

<#
.SYNOPSIS
    Adds the file or directory's short path as a "ShortPath" NoteProperty to each input object. 
.DESCRIPTION
    Adds the file or directory's short path as a "ShortPath" NoteProperty to each input object.
    NOTE: This filter requires the PSCX cmdlet Get-ShortPath
.PARAMETER InputObject
    A DirectoryInfo or FileInfo object on which to add the ShortPath property
.EXAMPLE
    C:\PS> Get-ChildItem | Add-ShortPath | Format-Table ShortPath,FullName
    This example shows how you can add the short path to each DirectoryInfo or FileInfo object in the pipeline.
#>
function Add-ShortPath
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [AllowNull()]
        [PSObject]
        $InputObject
    )

    Process {
        if ($InputObject -is [System.IO.FileSystemInfo]) {
            $shortPathInfo = Get-ShortPath -LiteralPath $_.Fullname 
            Add-Member NoteProperty ShortPath $shortPathInfo.ShortPath -InputObject $InputObject
        }
        $InputObject
    }
}

Export-ModuleMember -Alias * -Function *