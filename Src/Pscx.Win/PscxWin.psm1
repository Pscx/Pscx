# -----------------------------------------------------------------------
# Desc: This is the PscxWin initialization module script. This module is 
# not meant to be used standalone, only as a companion module for PSCX 
# when running on Windows OS
# -----------------------------------------------------------------------
Set-StrictMode -Version Latest

<#
.SYNOPSIS
    Resolves the hresult error code to a textual description of the error.
.DESCRIPTION
    Resolves the hresult error code to a textual description of the error.
.PARAMETER HResult
    The hresult error code to resolve.
.EXAMPLE
    C:\PS> Resolve-HResult -2147023293
    Fatal error during installation. (Exception from HRESULT: 0x80070643)
.NOTES
    Aliases:  rvhr
#>
function Resolve-HResult {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [long[]]
        $HResult
    )

    Process {
        foreach ($hr in $HResult) {
            $comEx = [System.Runtime.InteropServices.Marshal]::GetExceptionForHR($hr)
            if ($comEx) {
                $comEx.Message
            }
            else {
                Write-Error "$hr doesn't correspond to a known HResult"
            }
        }
    }
}

<#
.SYNOPSIS
    Resolves a Windows error number a textual description of the error.
.DESCRIPTION
    Resolves a Windows error number a textual description of the error. The Windows
    error number is typically retrieved via the Win32 API GetLastError() but it is
    typically displayed in messages to the end user.
.PARAMETER ErrorNumber
    The Windows error code number to resolve.
.EXAMPLE
    C:\PS> Resolve-WindowsError 5
    Access is denied
.NOTES
    Aliases:  rvwer
#>
function Resolve-WindowsError {
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [int[]]
        $ErrorNumber
    )

    Process {
        foreach ($num in $ErrorNumber) {
            $win32Ex = new-object ComponentModel.Win32Exception $num
            if ($win32Ex) {
                $win32Ex.Message
            }
            else {
                Write-Error "$num does not correspond to a known Windows error code"
            }
        }
    }
}

# set the 7zip library path
[SevenZip.SevenZipBase]::SetLibraryPath([System.IO.Path]::Join([Pscx.Core.PscxContext]::Instance.AppsDir, "7z.dll"))

# -----------------------------------------------------------------------
# Cmdlet aliases
# -----------------------------------------------------------------------

Export-ModuleMember -Alias * -Function * -Cmdlet *