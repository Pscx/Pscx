# ---------------------------------------------------------------------------
# Author: Keith Hill
# Desc:   Exports the relevant command types for comparison between 
#         PowerShell and different versions of PSCX.
# Usage:  .\ExportCommands.ps1 Pscx1_1Commnds.xml
# ---------------------------------------------------------------------------

param([string]$path=$(throw 'You must provide a path to a file to export the commands to.'))

gcm -type Alias, Function, Filter, Cmdlet, ExternalScript | Sort CommandType, Name | Export-Clixml $path