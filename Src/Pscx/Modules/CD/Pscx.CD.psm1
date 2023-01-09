#---------------------------------------------------------------------------
# Author: Keith Hill, Dan Luca
# Desc:   Module that replaces the regular CD function with one that handles
#         history and backward/forward navigation using - and +.
#         as ..[.]*.
# Date:   Nov 18, 2006; Dec 22, 2021
# Site:   https://www.gitlab.com/danluca/pscx-light
#---------------------------------------------------------------------------
#requires -Version 6
Set-StrictMode -Version Latest

$backwardStack = new-object System.Collections.ArrayList
$forewardStack = new-object System.Collections.ArrayList

# When the module removed, set the cd alias back to something reasonable.
# We could use the original cd alias but most of the time it's going to be set to Set-Location.
# And you may have loaded another module in between stashing the "original" cd alias that
# modifies the cd alias.  So setting it back to the "original" may not be the right thing to
# do anyway.
$ExecutionContext.SessionState.Module.OnRemove = {
    Set-Alias cd Set-Location -Scope Global -Option AllScope -Force
}.GetNewClosure()

# We are going to replace the PowerShell default "cd" alias with the CD function defined below.
Set-Alias cd Pscx\Set-PscxLocation -Force -Scope Global -Option AllScope -Description "PSCX: Enhanced set-location cmdlet with history stack"

<#
.SYNOPSIS
    Set-PscxLocation function that tracks location history allowing easy navigation to previous locations.
.DESCRIPTION
    Set-PscxLocation function that tracks location history allowing easy navigation to previous locations.
    Set-PscxLocation maintains a backward and forward stack mechanism that can be navigated using "Set-PscxLocation -"
    to go backwards in the stack and "Set-PscxLocation +" to go forwards in the stack.  Executing "Set-PscxLocation"
    without any parameters will display the current stack history. 
    
    By default, the new location is echo'd to the host.  If you want to suppress this set the preference 
    variable in your profile e.g. $Pscx:Preferences['CD_EchoNewLocation'] = $false. 
    
    If you want to change your cd alias to use Set-PscxLocation, execute:
    Set-Alias cd Set-PscxLocation -Option AllScope
.PARAMETER Path
    The path to change location to.
.PARAMETER LiteralPath
    The literal path to change location to.  This path can contain wildcard characters that
    do not need to be escaped.
.PARAMETER PassThru
    If the PassThru switch is specified the object passed into the Set-PscxLocation function is also output
    from the function.  This allows the next pipeline stage to also operate on the object.
.PARAMETER UnboundArguments
    This parameter accumulates all the additional arguments and concatenates them to the Path
    or LiteralPath parameter using a space separator.  This allows you to cd to some paths containing
    spaces without having to quote the path e.g. 'cd c:\program files'.  Note that this doesn't always
    work.  For example, this following won't work: 'cd c:\program files (x86)'.  This fails because
    PowerShell tries to evaluate the contents of the expression '(x86)' which isn't a valid command name.
.PARAMETER UseTransaction
    Includes the command in the active transaction. This parameter is valid only when a transaction
    is in progress. For more information, see about_Transactions.  This parameter is not supported
    in PowerShell Core.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; cd $pshome; cd -; cd +
    This example changes location to the PowerShell install dir, then back to the original
    location, than forward again to the PowerShell install dir.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; cd ....
    This example changes location up two levels from the current path.  You can use an arbitrary
    number of periods to indicate how many levels you want to go up.  A single period "." indicates
    the current location.  Two periods ".." indicate the current location's parent.  Three periods "..."
    indicates the current location's parent's parent and so on.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; cd
    Executing CD without any parameters will cause it to display the current stack contents.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; cd =0 OR cd *0
    Changes location to the very first (0th index) location in the stack. Execute CD without any parameters
    to see all the paths, then execute CD =<number> or CD *<number> to change location to that path.
.EXAMPLE
    C:\PS> cd +2; cd -3
    Changes location forward two entries incrementally in the stack (if the new stack position is valid). Then changes location
    backwards three entries incrementally in the stack.
.EXAMPLE
    C:\PS> cd %; cd *-; cd -*
    Changes location to the very first (0th index) location in the stack.
.EXAMPLE
    C:\PS> cd !; cd *+; cd +*
    Changes location to the very last location in the stack.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; $profile | cd
    This example will change location to the parent location of $profile.
.NOTES
    This is a PSCX function.
#>
function Set-PscxLocation {
    [CmdletBinding(DefaultParameterSetName = 'Path')]
    param(
        [Parameter(Position = 0, ParameterSetName = 'Path', ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
        [string]
        $Path,

        [Parameter(Position = 0, ParameterSetName = 'LiteralPath', ValueFromPipelineByPropertyName = $true)]
        [Alias("PSPath")]
        [string]
        $LiteralPath,

        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]
        $UnboundArguments,

        [Parameter()]
        [switch]
        $PassThru,

        [Parameter()]
        [switch]
        $UseTransaction
    )

    Begin {
        Set-StrictMode -Version Latest

        # String resources
        Import-LocalizedData -BindingVariable msgTbl -FileName Messages

        $ExtraArgs = @{}
        if (($PSVersionTable.PSVersion.Major -lt 6) -or ($PSVersionTable.PSEdition -eq 'Desktop')) {
            $ExtraArgs['UseTransaction'] = $UseTransaction
        }

        function SetLocationImpl($path, [switch]$IsLiteralPath) {
            if ($pscmdlet.ParameterSetName -eq 'LiteralPath' -or $IsLiteralPath) {
                Write-Debug "Setting location to literal path: '$path'"
                Set-Location -LiteralPath $path @ExtraArgs
            }
            else {
                Write-Debug "Setting location to path: '$path'"
                Set-Location $path @ExtraArgs
            }

            if ($PassThru) {
                Write-Output $ExecutionContext.SessionState.Path.CurrentLocation
            }
            else {
                # If not passing thru, then check for user options of other info to display.
                if ($Pscx:Preferences['CD_GetChildItem']) {
                    Get-ChildItem
                }
                elseif ($Pscx:Preferences['CD_EchoNewLocation']) {
                    Write-Host $ExecutionContext.SessionState.Path.CurrentLocation
                }
            }
        }

        $lnHorz = "`u{2594}"
        $lnSpace = "`u{2579}"
        #$lnHorz = "`u{2501}"
        #$marker = "`u{00bb}"
        $marker = "`u{276f}"
        $clrBold = "`e[1m"
        $clrNoBold = "`e[22m"
        $clrItalic = "`e[3m"
        $clrNoItalic = "`e[23m"
        $clrHeader = "`e[38;5;244m`e[48;5;236m"
        $clrCurrent = "`e[38;5;220m"
        $clrDefault = "`e[39m`e[49m"
        $clrReset = "`e[0m"
    }

    Process {
        if ($pscmdlet.ParameterSetName -eq 'Path') {
            Write-Debug "Path parameter received: '$Path'"
            $aPath = $Path
        }
        else {
            Write-Debug "LiteralPath parameter received: '$LiteralPath'"
            $aPath = $LiteralPath
        }

        if ($UnboundArguments -and $UnboundArguments.Count -gt 0) {
            $OFS = ','
            Write-Debug "Appending unbound arguments to path: '$UnboundArguments'"
            $aPath = $aPath + " " + ($UnboundArguments -join ' ')
        }

        # If no input, dump contents of backward and foreward stacks
        if (!$aPath) {
            [int]$i = 0
            # Command to dump the backward & foreward stacks
            "$clrReset"
            Write-Information " $clrHeader$clrBold   # Directory Stack:$(' '*17)$clrNoBold$clrDefault" -InformationAction Continue
            Write-Information " $clrHeader $($lnHorz * 3)$lnSpace$($lnHorz * 16)$(' '*17)$clrDefault" -InformationAction Continue
            if ($backwardStack.Count -ge 0) {
                for ($i = 0; $i -lt $backwardStack.Count; $i++) {
                    "   {0,3} {1}" -f $i, $backwardStack[$i]
                }
            }

            "$clrCurrent $marker{0,3} {1}$clrDefault" -f $i++, $ExecutionContext.SessionState.Path.CurrentLocation

            if ($forewardStack.Count -ge 0) {
                $ndx = $i
                for ($i = 0; $i -lt $forewardStack.Count; $i++) {
                    "   {0,3} {1}" -f ($ndx + $i), $forewardStack[$i]
                }
            }
            "$clrReset"
            return
        }

        Write-Debug "Processing arg: '$aPath'"

        $currentPathInfo = $ExecutionContext.SessionState.Path.CurrentLocation

        # Expand ..[.]+ out to ..\..[\..]+
        if ($aPath -like "*...*") {
            $regex = [regex]"\.\.\."
            while ($regex.IsMatch($aPath)) {
                $aPath = $regex.Replace($aPath, "..$([System.IO.Path]::DirectorySeparatorChar)..")
            }
        }

        switch ($aPath) {
            "-" {
                if ($backwardStack.Count -eq 0) {
                    Write-Warning $msgTbl.BackStackEmpty
                }
                else {
                    $lastNdx = $backwardStack.Count - 1
                    $prevPath = $backwardStack[$lastNdx]
                    SetLocationImpl $prevPath -IsLiteralPath
                    [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                    $backwardStack.RemoveAt($lastNdx)
                }
                break
            }
            "+" {
                if ($forewardStack.Count -eq 0) {
                    Write-Warning $msgTbl.ForeStackEmpty
                }
                else {
                    $nextPath = $forewardStack[0]
                    SetLocationImpl $nextPath -IsLiteralPath
                    [void]$backwardStack.Add($currentPathInfo.Path)
                    $forewardStack.RemoveAt(0)
                }
                break
            }
            { $_ -in "*-", "%", "-*" } {
                [int]$num = 0
                $backstackSize = $backwardStack.Count
                $forestackSize = $forewardStack.Count
                if ($num -eq $backstackSize) {
                    Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
                }
                else {
                    $selectedPath = $backwardStack[$num]
                    SetLocationImpl $selectedPath -IsLiteralPath
                    [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                    $backwardStack.RemoveAt($num)

                    if ($backwardStack.Count -gt 0) {
                        $forewardStack.InsertRange(0, $backwardStack)
                        $backwardStack.Clear()
                    }
                }
                break
            }
            { $_ -in "*+", "!", "+*" } {
                $backstackSize = $backwardStack.Count
                $forestackSize = $forewardStack.Count
                [int]$num = $backstackSize + $forestackSize
                if ($num -eq $backstackSize) {
                    Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
                }
                else {
                    [int]$ndx = $forestackSize - 1
                    $selectedPath = $forewardStack[$ndx]
                    SetLocationImpl $selectedPath -IsLiteralPath
                    [void]$backwardStack.Add($currentPathInfo.Path)
                    $forewardStack.RemoveAt($ndx)

                    if ($ndx -gt 0) {
                        $backwardStack.InsertRange(($backwardStack.Count), $forewardStack)
                        $forewardStack.Clear()
                    }
                }
                break
            }
            default {
                switch -Wildcard ($aPath) {
                    "[=*][0-9]*" {
                        [int]$num = $aPath.Substring(1)
                        $backstackSize = $backwardStack.Count
                        $forestackSize = $forewardStack.Count
                        if ($num -eq $backstackSize) {
                            Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
                        }
                        elseif ($num -lt $backstackSize) {
                            $selectedPath = $backwardStack[$num]
                            SetLocationImpl $selectedPath -IsLiteralPath
                            [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                            $backwardStack.RemoveAt($num)

                            [int]$ndx = $num
                            [int]$count = $backwardStack.Count - $ndx
                            if ($count -gt 0) {
                                $itemsToMove = $backwardStack.GetRange($ndx, $count)
                                $forewardStack.InsertRange(0, $itemsToMove)
                                $backwardStack.RemoveRange($ndx, $count)
                            }
                        }
                        elseif (($num -gt $backstackSize) -and ($num -lt ($backstackSize + 1 + $forestackSize))) {
                            [int]$ndx = $num - ($backstackSize + 1)
                            $selectedPath = $forewardStack[$ndx]
                            SetLocationImpl $selectedPath -IsLiteralPath
                            [void]$backwardStack.Add($currentPathInfo.Path)
                            $forewardStack.RemoveAt($ndx)

                            [int]$count = $ndx
                            if ($count -gt 0) {
                                $itemsToMove = $forewardStack.GetRange(0, $count)
                                $backwardStack.InsertRange(($backwardStack.Count), $itemsToMove)
                                $forewardStack.RemoveRange(0, $count)
                            }
                        }
                        else {
                            Write-Warning ($msgTbl.NumOutOfRangeF1 -f $num)
                        }
                        break
                    }
                    "-[0-9]*" {
                        [int]$num = $aPath.Substring(1)
                        $backstackSize = $backwardStack.Count
                        if ($num -eq 0) {
                            Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
                        }
                        elseif ($num -le $backstackSize) {
                            [int]$ndx = $backstackSize - $num
                            $selectedPath = $backwardStack[$ndx]
                            SetLocationImpl $selectedPath -IsLiteralPath
                            [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                            $backwardStack.RemoveAt($ndx)

                            [int]$count = $num - 1
                            if ($count -gt 0) {
                                $itemsToMove = $backwardStack.GetRange($ndx, $count)
                                $forewardStack.InsertRange(0, $itemsToMove)
                                $backwardStack.RemoveRange($ndx, $count)
                            }
                        }
                        else {
                            Write-Warning ($msgTbl.NumOutOfRangeF1 -f $num)
                        }
                        break
                    }
                    "+[0-9]*" {
                        [int]$num = $aPath.Substring(1)
                        $forestackSize = $forewardStack.Count
                        if ($num -eq 0) {
                            Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
                        }
                        elseif ($num -le $forestackSize) {
                            [int]$ndx = $num - 1
                            $selectedPath = $forewardStack[$ndx]
                            SetLocationImpl $selectedPath -IsLiteralPath
                            [void]$backwardStack.Add($currentPathInfo.Path)
                            $forewardStack.RemoveAt($ndx)

                            if ($ndx -gt 0) {
                                $itemsToMove = $forewardStack.GetRange(0, $ndx)
                                $backwardStack.AddRange($itemsToMove)
                                $forewardStack.RemoveRange(0, $ndx)
                            }
                        }
                        else {
                            Write-Warning ($msgTbl.NumOutOfRangeF1 -f $num)
                        }
                        break
                    }
                    default {
                        $driveName = ''
                        if ($ExecutionContext.SessionState.Path.IsPSAbsolute($aPath, [ref]$driveName) -and !(Test-Path -LiteralPath $aPath -PathType Container)) {
                            # File or a non-existant path - handle the case of "cd $profile" when the profile script doesn't exist
                            $aPath = Split-Path $aPath -Parent
                            Write-Debug "Path is not a container, attempting to set location to parent: '$aPath'"
                        }

                        SetLocationImpl $aPath

                        $forewardStack.Clear()

                        # Don't add the same path twice in a row
                        $newPathInfo = $ExecutionContext.SessionState.Path.CurrentLocation
                        if (($currentPathInfo.Provider -eq $newPathInfo.Provider) -and ($currentPathInfo.ProviderPath -eq $newPathInfo.ProviderPath)) {
                            break
                        }
                        [void]$backwardStack.Add($currentPathInfo.Path)
                        break
                    }
                }
            }
        }
    }
}
