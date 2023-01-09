# -----------------------------------------------------------------------
# Desc: This is the PscxWin initialization module script. This module is 
# not meant to be used standalone, only as a companion module for PSCX 
# when running on Windows OS
# -----------------------------------------------------------------------
Set-StrictMode -Version Latest

<#
.SYNOPSIS
    Runs the specified command in an elevated context.
.DESCRIPTION
    Runs the specified command in an elevated context.  This is useful on
    Windows 7 when you run with a standard user token but can elevate to Admin when needed.
.EXAMPLE
    C:\PS> Invoke-Elevated
    Opens a new PowerShell instance as admin.
.EXAMPLE
    C:\PS> Invoke-Elevated Notepad C:\windows\system32\drivers\etc\hosts
    Opens notepad elevated with the hosts file so that you can save changes to the file.
.EXAMPLE
    C:\PS> Invoke-Elevated {gci c:\windows\temp | export-clixml tempdir.xml; exit}
    Executes the scriptblock in an elevated PowerShell instance.
.EXAMPLE
    C:\PS> Invoke-Elevated {gci c:\windows\temp | export-clixml tempdir.xml; exit} | %{$_.WaitForExit(5000)} | %{Import-Clixml tempdir.xml}
    Executes the scriptblock in an elevated PowerShell instance, waits for that elevated process to execute, then
    retrieves the results.
.NOTES
    Aliases:  su
    Author:   Keith Hill
#>
function Invoke-Elevated() {
    Write-Debug "`$MyInvocation:`n$($MyInvocation | Out-String)"

    $OFS=" "
    # if already elevated, just run in the same process what is given to this command
    if ($Pscx:IsAdmin) {
        if ($args.Count -eq 0) {
            Write-Output "This session is already elevated"
        } else {
            [string[]]$cmdArgs = @()
            if ($args.Count -gt 1) {
                $cmdArgs = $args[1..$($args.Length - 1)]
            }
            if ($args[0] -is [scriptblock]) {
                & $args[0] $cmdArgs
            } else {
                $app = Get-Command $args[0] | Select -First 1 | Where { $_.CommandType -eq 'Application' }
                if ($app) {
                    & $app.Path $cmdArgs
                } else {
                    & $args[0] $cmdArgs
                }
            }
        }
        return
    }

    $escapedPath = [System.Management.Automation.Language.CodeGeneration]::EscapeSingleQuotedStringContent($pwd.ProviderPath)

    $startProcessArgs = @{
        FilePath     = (Get-Process -id $PID).Path
        ArgumentList = "-NoExit", "-Command", "& {Set-Location '$escapedPath'}"
        Verb         = "runas"
        PassThru     = $true
        WorkingDir   = $pwd
    }

    if ($args.Count -eq 0) {
        Write-Debug "  Starting Powershell with no supplied args"
    }
    elseif ($args[0] -is [Scriptblock]) {
        $script = $args[0]
        if ($script -match '(?si)\s*param\s*\(') {
            $startProcessArgs['ArgumentList'] = "-NoExit", "-Command", "& {$script}"
        } else {
            $startProcessArgs['ArgumentList'] = "-NoExit", "-Command", "& {Set-Location '$escapedPath'; $script}"
        }
        [string[]]$cmdArgs = @()
        if ($args.Count -gt 1) {
            $cmdArgs = $args[1..$($args.Length - 1)]
            $startProcessArgs['ArgumentList'] += $cmdArgs
        }
        Write-Debug "  Starting PowerShell with scriptblock: {$script} and args: $cmdArgs"
    }
    else {
        $app = Get-Command $args[0] | Select -First 1 | Where { $_.CommandType -eq 'Application' }
        [string[]]$cmdArgs = @()
        if ($args.Count -gt 1) {
            $cmdArgs = $args[1..$($args.Length - 1)]
        }
        if ($app) {
            $startProcessArgs['FilePath'] = $app.Path
            if ($cmdArgs.Count -eq 0) {
                $startProcessArgs.Remove('ArgumentList')
            } else {
                $startProcessArgs['ArgumentList'] = $cmdArgs
            }
            Write-Debug "  Starting app $app with args: $cmdArgs"
        }
        else {
            $poshCmd = $args[0]
            $startProcessArgs['ArgumentList'] = "-NoExit", "-Command", "& {Set-Location '$escapedPath'; $poshCmd $cmdArgs}"
            Write-Debug "  Starting $(Split-Path $startProcessArgs['FilePath'] -Leaf) command $poshCmd with args: $cmdArgs"
        }
    }

    Write-Debug "  Invoking Start-Process with args: $($startProcessArgs | Format-List | Out-String)"
    Microsoft.PowerShell.Management\Start-Process @startProcessArgs
}


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


# -----------------------------------------------------------------------
# Cmdlet aliases
# -----------------------------------------------------------------------

Export-ModuleMember -Alias * -Function * -Cmdlet *