@{
    GUID               = '0fab0d39-2f29-4e79-ab9a-fd750c66e6c5'
    Author             = 'PowerShell Core Community Extensions Team'
    CompanyName        = 'PowerShell Core Community Extensions'
    Copyright          = 'Copyright PowerShell Core Community Extensions Team 2006 - 2023.'
    Description        = 'PowerShell Core Community Extensions (PSCX) base module which implements a general purpose set of Cmdlets.'
    PowerShellVersion  = '7.2'
    CLRVersion         = '6.0'
    ModuleVersion      = "3.6.4"
    RequiredAssemblies = 'Pscx.dll' # needed for [pscxmodules] type (does not import cmdlets/providers)
    RootModule         = 'Pscx.psm1'
    NestedModules      = 'Pscx.dll'
    AliasesToExport    = '*'
    CmdletsToExport    = @(
        # PSCX main module
        'Join-PscxString',
        'Get-DriveInfo',
        'Format-Byte',
        'ConvertTo-Base64',
        'Format-Xml',
        'Set-ForegroundWindow',
        'Get-ForegroundWindow',
        'Get-LoremIpsum',
        'Format-Hex',
        'Get-PEHeader',
        'Test-Script',
        'Test-Xml',
        'Get-EnvironmentBlock',
        'Get-TypeName',
        'ConvertTo-WindowsLineEnding',
        'Get-FileTail',
        'Get-PathVariable',
        'Pop-EnvironmentBlock',
        'Get-FileVersionInfo',
        'Convert-Xml',
        'ConvertTo-Metric',
        'Set-FileTime',
        'Split-PscxString',
        'ConvertTo-UnixLineEnding',
        'Get-PscxHash',
        'Edit-File',
        'Test-Assembly',
        'Set-PathVariable',
        'Push-EnvironmentBlock',
        'Add-PathVariable',
        'ConvertFrom-Base64',
        'Skip-Object',
        'ConvertTo-MacOs9LineEnding',
        #PSCXWin module
        'Invoke-OleDbCommand',
        'Remove-ReparsePoint',
        'Get-RunningObject',
        'Get-SqlData',
        'Test-UserGroupMembership',
        'Get-DomainController',
        'Get-AdoDataProvider',
        'Get-OleDbData',
        'Write-PscxArchive',
        'Read-PscxArchive',
        'Get-ShortPath',
        'Expand-PscxArchive',
        'Get-AdoConnection',
        'Get-PscxADObject',
        'Get-Privilege',
        'Remove-MountPoint',
        'Get-MountPoint',
        'Invoke-AdoCommand',
        'New-Shortcut',
        'Get-OleDbDataSet',
        'Set-VolumeLabel',
        'New-Hardlink',
        'Get-OpticalDriveInfo',
        'Get-ReparsePoint',
        'Set-Privilege',
        'Invoke-SqlCommand',
        'New-Symlink',
        'New-Junction',
        'Get-PscxUptime',
        'Get-SqlDataSet',
        'Invoke-Apartment',
        'Disconnect-TerminalSession',
        'Stop-TerminalSession',
        'Get-TerminalSession',
        'Get-DhcpServer'    
    )
    FunctionsToExport = @(
        #PSCX.CD
        'Set-PscxLocation',
        #PSCX.FileSystem
        'Add-DirectoryLength',
        'Add-ShortPath',
        #PSCX.Utility
        'AddAccelerator',
        'PscxHelp',
        'PscxLess',
        'Edit-Profile',
        'Edit-HostProfile',
        'Resolve-ErrorRecord',
        'Resolve-HResult',
        'Resolve-WindowsError',
        'QuoteList',
        'QuoteString',
        'Invoke-GC',
        'Invoke-BatchFile',
        'Get-ViewDefinition',
        'Stop-RemoteProcess',
        'Get-ScreenCss',
        'Get-ScreenHtml',
        'Invoke-Method',
        'Set-Writable',
        'Set-FileAttributes',
        'Set-ReadOnly',
        'Show-Tree',
        'Get-Parameter',
        'Import-VisualStudioVars',
        'Get-ExecutionTime',
        'AddRegex',
        #PSCX.Vhd
        'Mount-PscxVHD',
        'Dismount-PscxVHD',    
        #PSCXWin
        'Resolve-HResult',
        'Resolve-WindowsError',
        #PSCXWin.Sudo
        'sudo', 
        'invoke-sudo'
    )
    FormatsToProcess   = @(
        'FormatData\Pscx.Environment.Format.ps1xml',
        'FormatData\Pscx.Format.ps1xml',
        'FormatData\Pscx.SIUnits.Format.ps1xml'
    )
    TypesToProcess     = @(
        'TypeData\Pscx.Reflection.Type.ps1xml',
        'TypeData\Pscx.SIUnits.Type.ps1xml'
    )

    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{

        PSData = @{

            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('Utilities','Xml','Base64','PEHeader','CD')

            # A URL to the license for this module.
            LicenseUri = 'https://github.com/danluca/Pscx/blob/master/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/danluca/Pscx'

            # A URL to an icon representing this module.
            IconUri = 'https://github.com/danluca/Pscx/blob/master/PscxIcon.png?raw=true'

            #Prerelease = 'beta4'

            # Release notes
            ReleaseNotes = @'
4.0.0-beta4 - January 8, 2022

* BREAKING CHANGE: Remove Windows specific commands: *-MSMQueue, will consider moving into a stand-alone module for Windows only
* Expand sort alias to Sort-Object in PS1 files - fixes Get-Parameter on Linux/macOS

4.0.0-beta3 - January 2, 2022

* Updated Import-VisualStudioVars to support Visual Studio 2022. Thanks @weloytty (Bill Loytty)!

4.0.0-beta2 - October 22, 2020

* Renamed less function to PscxLess.
* Renamed help function to PscxHelp.
* Renamed prompt function to PscxPrompt.
* Renamed Get-ADObject to Get-PscxADObject.
* Renamed Get-Help to Get-PscxHelp.
* Renamed Mount/Dismount-VHD to Mount/Dismount-PscxVHD.

* Changed Pscx to only override the built-in help function if PageHelpUsingLess Pscx.UserPreference is $true
* Changed default value of Pscx.UserPreference to be $true only on PowerShell v5.

4.0.0-beta1 - October 17, 2020

BREAKING CHANGES - PLEASE READ
* Migrate to .NET 4.61
* Renamed Expand-Archive to Expand-PscxArchive and Read-Archive to Read-PscxArchive.
* Renamed Set-LocationEx to Set-PscxLocation.
* Renamed all *-Clipboard commands to *-PscxClipboard
* Renamed Format-Hex command to Format-PscxHex.
* Renamed Get-Uptime to Get-PscxUptime.
* Renamed Join-String to Join-PscxString.

* Removed redefinition of the cd alias
* Removed the gcb alias that now conflicts with the built-in gcb alias
* Removed ?? alias to avoid conflict with ?? operator in PS 7.
* Removed ?: alias since PS 7 now implements a true ternary operator.

* Fixed Expand-PscxArchive help topic to remove references to the Format parameter - this parameter does not exist.
* Changed help function to default to displaying Full help details.

3.6.0 - December 2022
* Upgraded to .NET 6.0, PowerShell Core 7.2
* Updates to CD module, jump to top/bottom of stack

3.5.0 - September 20, 2021

* Upgraded to .NET 5.0, PowerShell Core 7.1
* Extracted out Windows specific cmdlets & supporting code into dedicated assembly
# Fix New-*Link cmdlets due to path parameter constraints errors
* Removed more of the obscure cmdlets that are unlikely to be used

3.4.0 - March 10, 2020

* Ported to PowerShell Core and reduced the number of cmdlets

3.3.2 - January 16, 2018

* Fix Edit-File does not respect TextEditor property [#48](https://github.com/Pscx/Pscx/issues/48)

3.3.1 - October 12, 2017

* Fix Import-VisualStudioVars - Select-VSSetupInstance ignores VS 2017 Build Tools by default [#36](https://github.com/Pscx/Pscx/issues/36)
# Fix Import-VisualStudioVars - VS 2015 Build Tools do not have VsDevCmd.bat [#37](https://github.com/Pscx/Pscx/issues/37)
# Fix Import-VisualStudioVars fails when workload for VC is not installed [#41](https://github.com/Pscx/Pscx/issues/41)

3.3.0 - September 5, 2017

* Fix issues with CD functionality not working on PowerShell Core.
* Updated Import-VisualStudioVars to support Visual Studio 2017.
'@
        } # End of PSData hashtable
    } # End of PrivateData hashtable
}
