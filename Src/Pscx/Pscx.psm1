# -----------------------------------------------------------------------
# Desc: This is the PSCX initialization module script that loads nested
#       modules.  Which nested modules are loaded is controlled via
#       $Pscx:Preferences.ModulesToImport.  You can override the default
#       settings by passing either a file containing the appropriate
#       settings in hashtable form as shown in Pscx.Options.ps1 or you
#       can pass in a hashtable with the appropriate settings directly.
# -----------------------------------------------------------------------
Set-StrictMode -Version Latest

# -----------------------------------------------------------------------
# Displays help usage
# -----------------------------------------------------------------------
function WriteUsage([string]$msg)
{
    $moduleNames = $Pscx:Preferences.ModulesToImport.Keys | Sort

    if ($msg) { Write-Host $msg }

    $OFS = ','
    Write-Host @"

To load all PSCX modules using the default PSCX preferences execute:

    Import-Module Pscx

To load all PSCX modules except a few, pass in a hashtable containing
a nested hashtable called ModulesToImport.  In this nested hashtable
add the module name you want to suppress and set its value to false e.g.:

    Import-Module Pscx -args @{ModulesToImport = @{DirectoryServices = $false}}

To have complete control over which PSCX modules load as well as the PSCX
options, copy the Pscx.UserPreferences.ps1 file to your home dir. Edit this
file and modify the settings as desired.  Then pass the path to this file as
an argument to Import-Module as shown below:

    Import-Module Pscx -arg ~\Pscx.UserPreferences.ps1

The nested module names are:

$moduleNames

"@
}

# -----------------------------------------------------------------------
# Overwrites the default PSCX preferences with user specified preferences
# -----------------------------------------------------------------------
function UpdateDefaultPreferencesWithUserPreferences([hashtable]$userPreferences)
{
    # Walk the user specified settings and overwrite the defaults with them
    foreach ($key in $userPreferences.Keys)
    {
        if (!$Pscx:Preferences.ContainsKey($key))
        {
            Write-Warning "$key is not a recognized PSCX preference"
            continue
        }

        if ($key -eq 'ModulesToImport')
        {
            foreach ($modkey in $userPreferences.ModulesToImport.Keys)
            {
                if ($Pscx:Preferences.ModulesToImport.ContainsKey($modkey))
                {
                    $Pscx:Preferences.ModulesToImport.$modkey = $userPreferences.ModulesToImport.$modkey
                }
                else
                {
                    Write-Warning "$modkey is not a recognized PSCX nested module"
                }
            }
        }
        else
        {
            $Pscx:Preferences.$key = $userPreferences.$key
        }
    }
}

# -----------------------------------------------------------------------
# Process module arguments - allows user to override the default options
# using Import-Module -args
# -----------------------------------------------------------------------
if ($args.Length -gt 0)
{
    if ($args[0] -eq 'help')
    {
        # Display help/usage info
        WriteUsage
        return
    }
    elseif ($args[0] -is [hashtable])
    {
        # Hashtable of settings passed directly
        UpdateDefaultPreferencesWithUserPreferences $args[0]
    }
    elseif (Test-Path $args[0])
    {
        # Attempt to load the user specified settings by executing the specified script
        $userPreferences = & $args[0]
        if ($userPreferences -isnot [hashtable])
        {
            WriteUsage "'$($args[0])' must return a hashtable instead of a $($userPreferences.GetType().FullName)"
            return
        }

        UpdateDefaultPreferencesWithUserPreferences $userPreferences
    }
    else
    {
        # Display help/usage info
        WriteUsage "'$($args[0])' is not recognized as either a hashtable or a valid path"
        return
    }
}

# -----------------------------------------------------------------------
# Cmdlet aliases
# -----------------------------------------------------------------------
Set-Alias gtn   Pscx\Get-TypeName      -Description "PSCX alias"
Set-Alias fhex  Pscx\Format-Hex        -Description "PSCX alias"
Set-Alias cvxml Pscx\Convert-Xml       -Description "PSCX alias"
Set-Alias fxml  Pscx\Format-Xml        -Description "PSCX alias"
Set-Alias lorem Pscx\Get-LoremIpsum    -Description "PSCX alias"
Set-Alias ln    Pscx\New-HardLink      -Description "PSCX alias"
Set-Alias touch Pscx\Set-FileTime      -Description "PSCX alias"
Set-Alias tail  Pscx\Get-FileTail      -Description "PSCX alias"
Set-Alias skip  Pscx\Skip-Object       -Description "PSCX alias"

# Compatibility alias
# Set-Alias Resize-Bitmap Pscx\Set-BitmapSize -Description "PSCX alias"

# -----------------------------------------------------------------------
# Load the PscxWin companion module if running on Windows
# -----------------------------------------------------------------------
if ($IsWindows) {
    $subModuleBasePath = "$PSScriptRoot\PscxWin.psd1"
    try {
        # Don't complain about non-standard verbs with nested imports but we will still have one complaint for the final global scope import
        Import-Module $subModuleBasePath -DisableNameChecking
    } catch {
        Write-Warning "Module PscxWin load error: $_"
    }
}

if ($Pscx:Preferences["PageHelpUsingLess"]) {
    if ($PSVersionTable.PSVersion.Major -le 5) {
        Set-Alias help PscxHelp -Option AllScope -Scope Global -Description "PSCX alias"
    }
    elseif (!(Test-Path Env:PAGER)) {
        # Only set this env var if someone has not defined it themselves
        $env:PAGER = 'less'
        $env:LESS = "-PsPage %db?B of %D:.\. Press h for help or q to quit\.$"
    }
}

# -----------------------------------------------------------------------
# Load nested modules selected by user - on Windows, PscxWin must be loaded first
# -----------------------------------------------------------------------
$stopWatch = new-object System.Diagnostics.StopWatch
$keys = @($Pscx:Preferences.ModulesToImport.Keys)
if ($Pscx:Preferences.ShowModuleLoadDetails)
{
    Write-Host "PowerShell Core Community Extensions $($Pscx:Version)`n"
    $totalModuleLoadTimeMs = 0
    $stopWatch.Reset()
    $stopWatch.Start()
    $keys = @($keys | Sort)
}

foreach ($key in $keys)
{
    if ($Pscx:Preferences.ShowModuleLoadDetails)
    {
        $stopWatch.Reset()
        $stopWatch.Start()
        Write-Host " $key $(' ' * (20 - $key.length))[ " -NoNewline
    }

    if (!$Pscx:Preferences.ModulesToImport.$key)
    {
        # Not selected for loading by user
        if ($Pscx:Preferences.ShowModuleLoadDetails)
        {
            Write-Host "Skipped" -nonew
        }
    }
    else
    {
        $subModuleBasePath = "$PSScriptRoot\Modules\{0}\Pscx.{0}" -f $key

        # Check for PSD1 first
        $path = "$subModuleBasePath.psd1"
        if (!(Test-Path -PathType Leaf $path))
        {
            # Assume PSM1 only
            $path = "$subModuleBasePath.psm1"
            if (!(Test-Path -PathType Leaf $path))
            {
                # Missing/invalid module
                if ($Pscx:Preferences.ShowModuleLoadDetails)
                {
                    Write-Host "Module $path is missing ]"
                }
                else
                {
                    Write-Warning "Module $path is missing."
                }
                continue
            }
        }

        try
        {
            # Don't complain about non-standard verbs with nested imports but
            # we will still have one complaint for the final global scope import
            Import-Module $path -DisableNameChecking

            if ($Pscx:Preferences.ShowModuleLoadDetails)
            {
                $stopWatch.Stop()
                $totalModuleLoadTimeMs += $stopWatch.ElapsedMilliseconds
                $loadTimeMsg = "Loaded in {0,4} mS" -f $stopWatch.ElapsedMilliseconds
                Write-Host $loadTimeMsg -nonew
            }
        }
        catch
        {
            # Problem in module
            if ($Pscx:Preferences.ShowModuleLoadDetails)
            {
                Write-Host "Module $key load error: $_" -nonew
            }
            else
            {
                Write-Warning "Module $key load error: $_"
            }
        }
    }

    if ($Pscx:Preferences.ShowModuleLoadDetails)
    {
        Write-Host " ]"
    }
}

if ($Pscx:Preferences.ShowModuleLoadDetails)
{
    Write-Host "`nTotal module load time: $totalModuleLoadTimeMs mS"
}

Remove-Item Function:\WriteUsage
Remove-Item Function:\UpdateDefaultPreferencesWithUserPreferences
Export-ModuleMember -Alias * -Function * -Cmdlet *
