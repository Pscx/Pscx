#---------------------------------------------------------------------------
# Author: Keith Hill
# Desc:   Module that replaces the regular CD function with one that handles
#         history and backward/forward navigation using - and +.
#         as ..[.]*.
# Date:   Nov 18, 2006, Oct 17, 2020
# Site:   https://github.com/Pscx/Pscx
#---------------------------------------------------------------------------
#requires -Version 3
Set-StrictMode -Version Latest

$backwardStack = new-object System.Collections.ArrayList
$forewardStack = new-object System.Collections.ArrayList

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
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; cd -0
    Changes location to the very first (0th index) location in the stack. Execute CD without any parameters
    to see all the paths, then execute CD -<number> to change location to that path.
.EXAMPLE
    C:\PS> set-alias cd Set-PscxLocation -Option AllScope; $profile | cd
    This example will change location to the parent location of $profile.
.NOTES
    This is a PSCX function.
#>
function Set-PscxLocation
{
    [CmdletBinding(DefaultParameterSetName='Path')]
    param(
        [Parameter(Position=0, ParameterSetName='Path', ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
        [string]
        $Path,

        [Parameter(Position=0, ParameterSetName='LiteralPath', ValueFromPipelineByPropertyName=$true)]
        [Alias("PSPath")]
        [string]
        $LiteralPath,

        [Parameter(ValueFromRemainingArguments=$true)]
        [string[]]
        $UnboundArguments,

        [Parameter()]
        [switch]
        $PassThru,

        [Parameter()]
        [switch]
        $UseTransaction
    )

    Begin
    {
        Set-StrictMode -Version Latest

        # String resources
        Import-LocalizedData -BindingVariable msgTbl -FileName Messages

        $ExtraArgs = @{}
        if (($PSVersionTable.PSVersion.Major -lt 6) -or ($PSVersionTable.PSEdition -eq 'Desktop'))
        {
            $ExtraArgs['UseTransaction'] = $UseTransaction
        }

        function SetLocationImpl($path, [switch]$IsLiteralPath)
        {
            if ($pscmdlet.ParameterSetName -eq 'LiteralPath' -or $IsLiteralPath)
            {
                Write-Debug "Setting location to literal path: '$path'"
                Set-Location -LiteralPath $path @ExtraArgs
            }
            else
            {
                Write-Debug "Setting location to path: '$path'"
                Set-Location $path @ExtraArgs
            }

            if ($PassThru)
            {
                Write-Output $ExecutionContext.SessionState.Path.CurrentLocation
            }
            else
            {
                # If not passing thru, then check for user options of other info to display.
                if ($Pscx:Preferences['CD_GetChildItem'])
                {
                    Get-ChildItem
                }
                elseif ($Pscx:Preferences['CD_EchoNewLocation'])
                {
                    Write-Host $ExecutionContext.SessionState.Path.CurrentLocation
                }
            }
        }
    }

    Process
    {
        if ($pscmdlet.ParameterSetName -eq 'Path')
        {
            Write-Debug "Path parameter received: '$Path'"
            $aPath = $Path
        }
        else
        {
            Write-Debug "LiteralPath parameter received: '$LiteralPath'"
            $aPath = $LiteralPath
        }

        if ($UnboundArguments -and $UnboundArguments.Count -gt 0)
        {
            $OFS=','
            Write-Debug "Appending unbound arguments to path: '$UnboundArguments'"
            $aPath = $aPath + " " + ($UnboundArguments -join ' ')
        }

        # If no input, dump contents of backward and foreward stacks
        if (!$aPath)
        {
            # Command to dump the backward & foreward stacks
            ""
            "     # Directory Stack:"
            "   --- ----------------"
            if ($backwardStack.Count -ge 0)
            {
                for ($i = 0; $i -lt $backwardStack.Count; $i++)
                {
                    "   {0,3} {1}" -f $i, $backwardStack[$i]
                }
            }

            "-> {0,3} {1}" -f $i++,$ExecutionContext.SessionState.Path.CurrentLocation

            if ($forewardStack.Count -ge 0)
            {
                $ndx = $i
                for ($i = 0; $i -lt $forewardStack.Count; $i++)
                {
                    "   {0,3} {1}" -f ($ndx+$i), $forewardStack[$i]
                }
            }
            ""
            return
        }

        Write-Debug "Processing arg: '$aPath'"

        $currentPathInfo = $ExecutionContext.SessionState.Path.CurrentLocation

        # Expand ..[.]+ out to ..\..[\..]+
        if ($aPath -like "*...*")
        {
            $regex = [regex]"\.\.\."
            while ($regex.IsMatch($aPath))
            {
                $aPath = $regex.Replace($aPath, "..$([System.IO.Path]::DirectorySeparatorChar)..")
            }
        }

        if ($aPath -eq "-")
        {
            if ($backwardStack.Count -eq 0)
            {
                Write-Warning $msgTbl.BackStackEmpty
            }
            else
            {
                $lastNdx = $backwardStack.Count - 1
                $prevPath = $backwardStack[$lastNdx]
                SetLocationImpl $prevPath -IsLiteralPath
                [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                $backwardStack.RemoveAt($lastNdx)
            }
        }
        elseif ($aPath -eq "+")
        {
            if ($forewardStack.Count -eq 0)
            {
                Write-Warning $msgTbl.ForeStackEmpty
            }
            else
            {
                $nextPath = $forewardStack[0]
                SetLocationImpl $nextPath -IsLiteralPath
                [void]$backwardStack.Add($currentPathInfo.Path)
                $forewardStack.RemoveAt(0)
            }
        }
        elseif ($aPath -like "-[0-9]*")
        {
            [int]$num = $aPath.replace("-","")
            $backstackSize = $backwardStack.Count
            $forestackSize = $forewardStack.Count
            if ($num -eq $backstackSize)
            {
                Write-Host "`n$($msgTbl.GoingToTheSameDir)`n"
            }
            elseif ($num -lt $backstackSize)
            {
                $selectedPath = $backwardStack[$num]
                SetLocationImpl $selectedPath -IsLiteralPath
                [void]$forewardStack.Insert(0, $currentPathInfo.Path)
                $backwardStack.RemoveAt($num)

                [int]$ndx = $num
                [int]$count = $backwardStack.Count - $ndx
                if ($count -gt 0)
                {
                    $itemsToMove = $backwardStack.GetRange($ndx, $count)
                    $forewardStack.InsertRange(0, $itemsToMove)
                    $backwardStack.RemoveRange($ndx, $count)
                }
            }
            elseif (($num -gt $backstackSize) -and ($num -lt ($backstackSize + 1 + $forestackSize)))
            {
                [int]$ndx = $num - ($backstackSize + 1)
                $selectedPath = $forewardStack[$ndx]
                SetLocationImpl $selectedPath -IsLiteralPath
                [void]$backwardStack.Add($currentPathInfo.Path)
                $forewardStack.RemoveAt($ndx)

                [int]$count = $ndx
                if ($count -gt 0)
                {
                    $itemsToMove = $forewardStack.GetRange(0, $count)
                    $backwardStack.InsertRange(($backwardStack.Count), $itemsToMove)
                    $forewardStack.RemoveRange(0, $count)
                }
            }
            else
            {
                Write-Warning ($msgTbl.NumOutOfRangeF1 -f $num)
            }
        }
        else
        {
            $driveName = ''
            if ($ExecutionContext.SessionState.Path.IsPSAbsolute($aPath, [ref]$driveName) -and
                !(Test-Path -LiteralPath $aPath -PathType Container))
            {
                # File or a non-existant path - handle the case of "cd $profile" when the profile script doesn't exist
                $aPath = Split-Path $aPath -Parent
                Write-Debug "Path is not a container, attempting to set location to parent: '$aPath'"
            }

            SetLocationImpl $aPath

            $forewardStack.Clear()

            # Don't add the same path twice in a row
            if ($backwardStack.Count -gt 0)
            {
                $newPathInfo = $ExecutionContext.SessionState.Path.CurrentLocation
                if (($currentPathInfo.Provider     -eq $newPathInfo.Provider) -and
                    ($currentPathInfo.ProviderPath -eq $newPathInfo.ProviderPath))
                {
                    return
                }
            }
            [void]$backwardStack.Add($currentPathInfo.Path)
        }
    }
}
