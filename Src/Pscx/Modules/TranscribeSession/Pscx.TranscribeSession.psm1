# ---------------------------------------------------------------------------
# Author: Misc, most inspired by ~Clint, Alex Angelopoulos and Andrew Watt
# Desc:   Logs each PowerShell session via the Start-Transcipt functionality
#         to a unique filename in the Transcripts subdir of your 
#         WindowsPowerShell user directory.
# Site:   http://pscx.codeplex.com
# ---------------------------------------------------------------------------

<#
.SYNOPSIS
    Searches your session transcript files for the specified pattern.
.DESCRIPTION
    Searches your session transcript files for the specified pattern.
.PARAMETER Pattern
    Pattern to match which is a regex pattern.
.PARAMETER CurrentOnly
    Searches just the current transcript file.
.PARAMETER SimpleMatch
    Do not interpret Pattern as a regex.
.PARAMETER List
    Just list transcript filenames that match pattern instead of each line.
.PARAMETER PassThru
    Passes the MatchInfo object down the pipeline.
.PARAMETER NoCompact
    Prevents multiple whitespace characters from being compacted to a single space.
.EXAMPLE
    C:\PS> Search-Transcript Format-Xml
    Searches all transcript files for usages of the Format-Xml cmdlet.
#>
function Search-Transcript
{
    param(
        [Parameter(Position=0, Mandatory=$true)]
        [string]
        $Pattern,
        
        [switch]
        $CurrentOnly,
        
        [switch]
        $SimpleMatch, 
        
        [switch]
        $List
    ) 

    # Determine full path to transcript dir
    $TranscriptPath = $Pscx:Session['TranscribeSession_TranscriptPath']
    $TranscriptDir = Split-Path $TranscriptPath -Parent
    $TranscriptFile = Split-Path $TranscriptPath -Leaf

    # Don't log any of the search activity to the transcript because it results in
    # confusion when the results of previous searches turn up in the current search results.
    Stop-Transcript | Out-Null
        
    # Trap any errors in the following nested scope so that we can be assured of starting
    # the transcript back up.
    try
    {
        Push-Location $TranscriptDir
        $transcriptFiles = @($TranscriptFile)
        if (!$CurrentOnly -and $TranscriptDir) 
        {
            $transcriptFiles += Get-ChildItem *.txt -Exclude $TranscriptFile
        }
            
        Select-String $Pattern -Path $transcriptFiles -SimpleMatch:$SimpleMatch -List:$List
    }
    finally
    {
        Pop-Location
        Start-Transcript $Pscx:Session['TranscribeSession_TranscriptPath'] -Append | Out-Null
    }    
}

# ---------------------------------------------------------------------------
# Transcribe is only supported in console host.
# Only execute this code once per PowerShell session.  
# ---------------------------------------------------------------------------
if (($Host.Name -ne 'ConsoleHost') -or $Pscx:Session['TranscribeSession_Loaded']) 
{ 
    return
}

$Pscx:Session['TranscribeSession_Loaded'] = $true

# Create Transcripts dir under user's profile directory
$TranscriptDir = Join-Path (Split-Path $Profile -Parent) Transcripts
if (!(Test-Path $TranscriptDir)) {
    New-Item $TranscriptDir -ItemType Directory > $null
}

$TranscriptFile = "{0}-{1:0###}.txt" -f (Get-Date -Format yyyyMMdd-HHmm), $PID
$Pscx:Session['TranscribeSession_TranscriptPath'] = Join-Path $TranscriptDir $TranscriptFile
Start-Transcript $Pscx:Session['TranscribeSession_TranscriptPath']

Export-ModuleMember -Alias * -Function *