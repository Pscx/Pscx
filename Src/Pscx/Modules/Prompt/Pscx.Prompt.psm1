# ---------------------------------------------------------------------------
# Author: Keith Hill
# Desc:   Infrastructure for applying prompt themes.  Prompt themes affect
#         the PowerShell prompt string and optionally the host's window
#         title and a startup message.
# Date:   Nov 07, 2009
# Site:   http://pscx.codeplex.com
# Usage:  In your Pscx.UserPreferences.ps1 file, set the prompt theme to one
#         of: Modern, WinXP or Jachym.
# ---------------------------------------------------------------------------
Set-StrictMode -Version Latest

$Theme = @{
    # Host RawUI Colors
    HostBackgroundColor   = $null
    HostForegroundColor   = $null

    PromptBackgroundColor = $null
    PromptForegroundColor = $null

    #Host Private Data Colors
    PrivateData = @{
        ErrorForegroundColor    = $null  # Console = Red,      ISE = #FFFF0000
        ErrorBackgroundColor    = $null  # Console = Black,    ISE = #00FFFFFF
        WarningForegroundColor  = $null  # Console = Yellow,   ISE = #FFFF8C00
        WarningBackgroundColor  = $null  # Console = Black,    ISE = #00FFFFFF
        DebugForegroundColor    = $null  # Console = Yellow,   ISE = #FF0000FF
        DebugBackgroundColor    = $null  # Console = Black,    ISE = #00FFFFFF
        VerboseForegroundColor  = $null  # Console = Yellow,   ISE = #FF0000FF
        VerboseBackgroundColor  = $null  # Console = Black,    ISE = #00FFFFFF
        ProgressForegroundColor = $null  # Console = Yellow,   ISE = <not defined>
        ProgressBackgroundColor = $null  # Console = DarkCyan, ISE = <not defined>
    }

    # Behavior (ie scriptblocks)
    PromptScriptBlock            = $null
    StartupMessageScriptBlock    = $null
    UpdateWindowTitleScriptBlock = $null
}

# ---------------------------------------------------------------------------
# Updates the host's window title
# ---------------------------------------------------------------------------
function Update-HostWindowTitle
{
    if (!$Theme.UpdateWindowTitleScriptBlock) { return }

    if ($Theme.UpdateWindowTitleScriptBlock -is [scriptblock])
    {
        $title = & $Theme.UpdateWindowTitleScriptBlock
    }
    else
    {
        $title = "$($Theme.UpdateWindowTitleScriptBlock)"
    }

    $OFS = ''
    $Host.UI.RawUI.WindowTitle = "$title"
}

# ---------------------------------------------------------------------------
# Writes the startup message
# ---------------------------------------------------------------------------
function Write-StartupMessage
{
    if (!$Theme.StartupMessageScriptBlock) { return }

    if ($Theme.StartupMessageScriptBlock -is [scriptblock])
    {
        $message = & $Theme.StartupMessageScriptBlock
    }
    else
    {
        $message = "$($Theme.StartupMessageScriptBlock)"
    }

    if (!$Pscx:Preferences['ShowModuleLoadDetails'])
    {
        Clear-Host
    }

    if ($message)
    {
        $foreColor = $Host.UI.RawUI.ForegroundColor

        if ($Host.Name -eq 'ConsoleHost')
        {
            if ($Theme.PromptForegroundColor)
            {
                $foreColor = $Theme.PromptForegroundColor
            }
        }

        Write-Host $message -ForegroundColor $foreColor
    }
}

# ---------------------------------------------------------------------------
# Writes the prompt string directly to the host
# ---------------------------------------------------------------------------
function Write-Prompt($Id)
{
    # Default prompt
    $prompt = "PS $(Get-Location)>"
    if ($Theme.PromptScriptBlock -is [scriptblock])
    {
        $OFS = ''
        $prompt = "$(& $Theme.PromptScriptBlock $Id)"
    }
    elseif ($Theme.PromptScriptBlock -is [string])
    {
        $prompt = $Theme.PromptScriptBlock
    }

    if ($Host.Name -eq 'ConsoleHost')
    {
        if ($Host.UI.RawUI.CursorPosition.X -ne 0)
        {
            Write-Host
        }

        $foreColor = $Host.UI.RawUI.ForegroundColor
        if ($Theme.PromptForegroundColor)
        {
            $foreColor = $Theme.PromptForegroundColor
        }

        $backColor = $Host.UI.RawUI.BackgroundColor
        if ($Theme.PromptBackgroundColor)
        {
            $backColor = $Theme.PromptBackgroundColor
        }

        Write-Host $prompt -NoNewLine -ForegroundColor $foreColor -BackgroundColor $backColor
    }
    else
    {
        # For any other host besides powershell.exe, just return the prompt string
        "$prompt "
    }
}

# ---------------------------------------------------------------------------
# The replacment prompt function
# ---------------------------------------------------------------------------
function PscxPrompt
{
    $id = 0
    $histItem = Get-History -Count 1
    if ($histItem)
    {
        $id = $histItem.Id
    }

    if ($id -eq 0)
    {
        Write-StartupMessage
    }

    Write-Prompt ($id + 1)
    Update-HostWindowTitle
    return ' ' # If you don't return anything PowerShell gives you PS>
}

# ---------------------------------------------------------------------------
# Load the specified theme (default is Modern)
# ---------------------------------------------------------------------------
$themeName = $Pscx:Preferences.PromptTheme
$themePath = "$PSScriptRoot\Themes\$themeName.ps1"
if (!(Test-Path $themePath))
{
    Write-Warning "Theme '$themeName' not found, defaulting to Modern theme"
    $themePath = "$PSScriptRoot\Themes\Modern.ps1"
}
Write-Verbose "Applying prompt theme '$themeName'"
& $themePath $Theme

# ---------------------------------------------------------------------------
# Initialize host colors and window title
# ---------------------------------------------------------------------------
if ($Host.Name -eq 'ConsoleHost')
{
    if ($Theme.HostForegroundColor)
    {
        $Host.UI.RawUI.ForegroundColor = $Theme.HostForegroundColor
    }
    if ($Theme.HostBackgroundColor)
    {
        $Host.UI.RawUI.BackgroundColor           = $Theme.HostBackgroundColor
        $Host.PrivateData.ErrorBackgroundColor   = $Theme.HostBackgroundColor
        $Host.PrivateData.WarningBackgroundColor = $Theme.HostBackgroundColor
        $Host.PrivateData.DebugBackgroundColor   = $Theme.HostBackgroundColor
        $Host.PrivateData.VerboseBackgroundColor = $Theme.HostBackgroundColor
    }
    foreach ($key in $Theme.PrivateData.Keys)
    {
        if ($Theme.PrivateData.$key)
        {
            $Host.PrivateData.$key = $Theme.PrivateData.$key
        }
    }
}

# Update window title if a scriptblock has been provided
Update-HostWindowTitle

Export-ModuleMember -Function PscxPrompt
