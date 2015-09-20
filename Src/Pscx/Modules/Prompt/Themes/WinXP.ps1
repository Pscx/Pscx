# ---------------------------------------------------------------------------
# Author: Keith Hill
# Desc:   Prompt, colors and host window title updates suited to Windows XP
#         where folks tend to run with adminstrator privileges all the time.
# Date:   Nov 07, 2009
# Site:   http://pscx.codeplex.com
# Usage:  In your options hashtable place the following setting:
#
#         PromptTheme = 'WinXP'
# ---------------------------------------------------------------------------
#requires -Version 3
param([hashtable]$Theme)

Set-StrictMode -Version Latest

# ---------------------------------------------------------------------------
# Colors
# ---------------------------------------------------------------------------
# Configure console colors.  $null signifies to use the console default color
if ($Pscx:IsAdmin -and ([System.Environment]::OSVersion.Version.Major -gt 5)) {
    $Theme.PromptForegroundColor = 'Red'
}

# ---------------------------------------------------------------------------
# Prompt ScriptBlock
# ---------------------------------------------------------------------------
$Theme.PromptScriptBlock = {
    param($nextCommandId) 
    
    # Determine what nesting level we are at (if any)
    $nestingLevel = ''
    if ($nestedpromptlevel -ge 1) {
        $nestingLevel = " [Nested:${nestedpromptlevel}]"
    }
    
    $promptChar = '>'
    if ($Pscx:IsAdmin) {
        $promptChar = '#'
    }
    
    # Output prompt string
    "${nextCommandId}${nestingLevel}$promptChar"
}

# ---------------------------------------------------------------------------
# Window Title Update ScriptBlock
# ---------------------------------------------------------------------------
$Theme.UpdateWindowTitleScriptBlock = {
    # Pretty much everyone runs as admin on XP and below so displaying the
    # admin status in the title bar is a waste of space. Only do this on Vista.
    $isVistaOrHigher = ([System.Environment]::OSVersion.Version.Major -gt 5)	

    $adminStatus = ''
    if ($Pscx:IsAdmin -and $isVistaOrHigher) { $adminStatus = 'Admin: ' }
        
    $location = Get-Location
    $version = $PSVersionTable.PSVersion
    
    $bitness = ''
    if ([IntPtr]::Size -eq 8) {
        $bitness = ' (x64)'
    }
    elseif ($Pscx:IsWow64Process) {
        $bitness = ' (x86)'
    }
    
    "$adminStatus$location - Windows PowerShell $version$bitness"
}
