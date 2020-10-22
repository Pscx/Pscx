# ---------------------------------------------------------------------------
# Author: Keith Hill
# Desc:   Prompt, colors and host window title updates suited to UAC enabled
#         Windows Vista and Windows 7.  Elevated (admin) prompts are easy
#         distinguish from non-elevated prompts.
# Date:   Nov 07, 2009
# Site:   http://pscx.codeplex.com
# Usage:  In your options hashtable place the following setting:
#
#         PromptTheme = 'Modern'
# ---------------------------------------------------------------------------
#requires -Version 3
param([hashtable]$Theme)

Set-StrictMode -Version Latest

# ---------------------------------------------------------------------------
# Colors
# ---------------------------------------------------------------------------
$Theme.HostBackgroundColor   = 'Black'
$Theme.HostForegroundColor   = 'White'
$Theme.PromptForegroundColor = 'White'

# ---------------------------------------------------------------------------
# Prompt ScriptBlock
# ---------------------------------------------------------------------------
$Theme.PromptScriptBlock = {
    param($Id)

    if ($NestedPromptLevel) {
        new-object string ([char]0xB7), $NestedPromptLevel
    }

    $sepChar = '>' # [char]0xBB
    if ($Pscx:IsAdmin) {
        $sepChar = '#'
    }

    $path = ''
    "${Id}$path$sepChar"
}

# ---------------------------------------------------------------------------
# Window Title Update ScriptBlock
# ---------------------------------------------------------------------------
$Theme.UpdateWindowTitleScriptBlock = {
    $adminPrefix = ''
    if ($Pscx:IsAdmin) {
        $adminPrefix = 'Admin'
    }
    $location = Get-Location
    $version = "$($PSVersionTable.PSVersion.Major).$($PSVersionTable.PSVersion.Minor)"

    $bitness = ''
    if ([IntPtr]::Size -eq 8) {
        $bitness = ' (x64)'
    }
    elseif ($Pscx:IsWow64Process) {
        $bitness = ' (x86)'
    }

    "$adminPrefix $location - Windows PowerShell $version$bitness"
}

# ---------------------------------------------------------------------------
# Startup Message ScriptBlock
# ---------------------------------------------------------------------------
$Theme.StartupMessageScriptBlock = {
    $logo = "Windows PowerShell $($PSVersionTable.PSVersion)"
    if ([IntPtr]::Size -eq 8) {
        $logo += ' (x64)'
    }
    elseif ($Pscx:IsWow64Process)
    {
        $logo += ' (x86)'
    }
    $logo

    $user =	"`nLogged in on $([DateTime]::Now.ToString((Get-Culture))) as $($Pscx:WindowsIdentity.Name)"

    if ($Pscx:IsAdmin) {
        $user += ' (Elevated).'
    }
    else {
        $user += '.'
    }

    $user
}
