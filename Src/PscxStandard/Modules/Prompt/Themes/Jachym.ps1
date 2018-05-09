# ---------------------------------------------------------------------------
# Author: Jachymko
# Desc:   Jachym's prompt, colors and host window title updates.
# Date:   Nov 07, 2009
# Site:   http://pscx.codeplex.com
# Usage:  In your options hashtable place the following setting:
#
#         PromptTheme = 'Jachym'
# ---------------------------------------------------------------------------
#requires -Version 3
param([hashtable]$Theme)

Set-StrictMode -Version Latest

# ---------------------------------------------------------------------------
# Colors
# ---------------------------------------------------------------------------
$Theme.HostBackgroundColor   = if ($Pscx:IsAdmin) { 'DarkRed' } else { 'Black' }
$Theme.HostForegroundColor   = if ($Pscx:IsAdmin) { 'White'   } else { 'Gray'  }
$Theme.PromptForegroundColor = if ($Pscx:IsAdmin) { 'Yellow'  } else { 'White' }
$Theme.PrivateData.ErrorForegroundColor = if ($Pscx:IsAdmin) { 'DarkCyan' }

# ---------------------------------------------------------------------------
# Prompt ScriptBlock
# ---------------------------------------------------------------------------
$Theme.PromptScriptBlock = {
	param($Id) 
	
	if($NestedPromptLevel) 
	{
		new-object string ([char]0xB7), $NestedPromptLevel
	}
	
	"[$Id] $([char]0xBB)"	
}		

# ---------------------------------------------------------------------------
# Window Title Update ScriptBlock
# ---------------------------------------------------------------------------
$Theme.UpdateWindowTitleScriptBlock = {
	(Get-Location)
	'-'
	'Windows PowerShell'

	if($Pscx:IsAdmin) 
	{ 
		'(Administrator)' 
	}
	
	if ($Pscx:IsWow64Process)
	{
		'(x86)'
	}
}
