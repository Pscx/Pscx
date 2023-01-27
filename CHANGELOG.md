# PowerShell Community Extensions Light Changelog

## 3.6.4 - December 2022

* Upgraded to .NET 6.0, PowerShell Core 7.2
* Enhanced CD module, jump to top/bottom of stack, ANSI colors, more shortcuts
* Minimum PowerShell version is 7.0
* Deprecated more of the limited use cmdlets or marked obsoleted (especialy as they utility has been superseeded by new PoSh releases, or obsoleted by new .Net releases)
  * e.g. *-MSMQueue
* Removed Prompt submodule - not working, of no real use due to plenty options for PoSh prompts much more refined.
  * [ompgit](https://gitlab.com/danluca/ompgit)
  * cross-platform [oh-my-posh](https://github.com/JanDeDobbeleer/oh-my-posh)
  * old PowerShell [oh-my-posh](https://github.com/JanDeDobbeleer/oh-my-posh2)
* Expand sort alias to Sort-Object in PS1 files - fixes Get-Parameter on Linux/macOS
* Updated Import-VisualStudioVars to support Visual Studio 2022. Thanks @weloytty (Bill Loytty)!
* Renamed less function to PscxLess.
* Renamed help function to PscxHelp.
* Renamed Get-ADObject to Get-PscxADObject.
* Removed Get-Help submodule - not really working, the Utility submodule PscxHelp is more effective
* Renamed Mount/Dismount-VHD to Mount/Dismount-PscxVHD.
* Changed Pscx to only override the built-in help function if PageHelpUsingLess Pscx.UserPreference is $true
* Renamed Expand-Archive to Expand-PscxArchive and Read-Archive to Read-PscxArchive.
* Renamed Set-LocationEx to Set-PscxLocation.
* Removed all *-Clipboard commands - superseeded by built-in PowerShell utilities
* Renamed Format-Hex command to Format-PscxHex.
* Renamed Get-Uptime to Get-PscxUptime.
* Renamed Join-String to Join-PscxString.
* Removed the gcb alias that now conflicts with the built-in gcb alias
* Fixed Expand-PscxArchive help topic to remove references to the Format parameter - this parameter does not exist.
* Changed help function to default to displaying Full help details.

## 3.5.0 - September 20, 2021

* Upgraded to .NET 5.0, PowerShell Core 7.1
* Extracted out Windows specific cmdlets & supporting code into dedicated assembly
- Fix New-*Link cmdlets due to path parameter constraints errors
* Removed more of the obscure cmdlets that are unlikely to be used

## 3.4.0 - March 12, 2020

* Converted to PowerShell Core 7 compliance, MacOS friendly
* Trimmed down the number of cmdlets and features to most useful ones


## Older changes

* See the Release notes for version 3.3.2 and older at https://github.com/Pscx/Pscx/blob/master/ReleaseNotes.txt
