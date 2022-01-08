@{
    GUID               = '0fab0d39-2f29-4e79-ab9a-fd750c66e6c5'
    Author             = 'Keith Hill, Oisin Grehan, and contributors'
    CompanyName        = 'http://pscx.codeplex.com/'
    Copyright          = '(c) 2006 - 2020 Keith Hill, Oisin Grehan, and contributors'
    Description        = 'PowerShell Community Extensions (PSCX) base module which implements a general purpose set of Cmdlets.'
    PowerShellVersion  = '5.0'
    CLRVersion         = '4.0'
    ModuleVersion      = '4.0.0'
    RequiredAssemblies = 'Pscx.dll' # needed for [pscxmodules] type (does not import cmdlets/providers)
    RootModule         = 'Pscx.psm1'
    NestedModules      = 'Pscx.dll'
    AliasesToExport    = '*'
    CmdletsToExport    = @(
        'Add-PathVariable',
        'ConvertFrom-Base64',
        'ConvertTo-Base64',
        'ConvertTo-MacOs9LineEnding',
        'ConvertTo-Metric',
        'ConvertTo-UnixLineEnding',
        'ConvertTo-WindowsLineEnding',
        'Convert-Xml',
        'Disconnect-TerminalSession',
        'Edit-File',
        'Expand-PscxArchive',
        'Export-Bitmap',
        'Format-Byte',
        'Format-PscxHex',
        'Format-Xml',
        'Get-PscxADObject',
        'Get-AdoConnection',
        'Get-AdoDataProvider',
        'Get-PscxClipboard',
        'Get-DhcpServer',
        'Get-DomainController',
        'Get-DriveInfo',
        'Get-EnvironmentBlock',
        'Get-FileTail',
        'Get-FileVersionInfo',
        'Get-ForegroundWindow',
        'Get-Hash',
        'Get-HttpResource',
        'Get-LoremIpsum',
        'Get-MountPoint',
        'Get-OpticalDriveInfo',
        'Get-PathVariable',
        'Get-PEHeader',
        'Get-Privilege',
        'Get-PscxUptime',
        'Get-PSSnapinHelp',
        'Get-ReparsePoint',
        'Get-RunningObject',
        'Get-ShortPath',
        'Get-TerminalSession',
        'Get-TypeName',
        'Import-Bitmap',
        'Invoke-AdoCommand',
        'Invoke-Apartment',
        'Join-PscxString',
        'New-Hardlink',
        'New-Junction',
        'New-Shortcut',
        'New-Symlink',
        'Out-PscxClipboard',
        'Ping-Host',
        'Pop-EnvironmentBlock',
        'Push-EnvironmentBlock',
        'Read-PscxArchive',
        'Remove-MountPoint',
        'Remove-ReparsePoint',
        'Resolve-Host',
        'Send-SmtpMail',
        'Set-BitmapSize',
        'Set-PscxClipboard',
        'Set-FileTime',
        'Set-ForegroundWindow',
        'Set-PathVariable',
        'Set-Privilege',
        'Set-VolumeLabel',
        'Skip-Object',
        'Split-String',
        'Stop-TerminalSession',
        'Test-AlternateDataStream',
        'Test-Assembly',
        'Test-Script',
        'Test-UserGroupMembership',
        'Test-Xml',
        'Write-BZip2',
        'Write-PscxClipboard',
        'Write-GZip',
        'Write-Tar',
        'Write-Zip'
    )
    FunctionsToExport = @(
        'Add-DirectoryLength',
        'Add-ShortPath',
        'Edit-Profile',
        'Edit-HostProfile',
        'Enable-OpenPowerShellHere',
        'Get-ExecutionTime',
        'Get-PscxHelp',
        'Get-Parameter',
        'Get-PropertyValue',
        'Get-ScreenCss',
        'Get-ScreenHtml',
        'Get-ViewDefinition',
        'Import-VisualStudioVars',
        'Invoke-BatchFile',
        'Invoke-Elevated',
        'Invoke-GC',
        'Invoke-Method',
        'Invoke-NullCoalescing',
        'Invoke-Ternary',
        'New-HashObject',
        'Out-Speech',
        'PscxHelp',
        'PscxLess',
        'QuoteList',
        'QuoteString',
        'Resolve-ErrorRecord',
        'Resolve-HResult',
        'Resolve-WindowsError',
        'Search-Transcript',
        'Set-Writable',
        'Set-ReadOnly',
        'Show-Tree',
        'Start-PowerShell',
        'Stop-RemoteProcess',
        'Set-PscxLocation',
        'Dismount-PscxVHD',
        'Mount-PscxVHD'
    )
    FormatsToProcess   = @(
        'FormatData\Pscx.Format.ps1xml',
        'FormatData\Pscx.FeedStore.Format.ps1xml',
        'FormatData\Pscx.Archive.Format.ps1xml',
        'FormatData\Pscx.Environment.Format.ps1xml',
        'FormatData\Pscx.Security.Format.ps1xml',
        'FormatData\Pscx.SIUnits.Format.ps1xml',
        'FormatData\Pscx.TerminalServices.Format.ps1xml'
    )
    TypesToProcess     = @(
        'TypeData\Pscx.FeedStore.Type.ps1xml',
        'TypeData\Pscx.Archive.Type.ps1xml',
        'TypeData\Pscx.Reflection.Type.ps1xml',
        'TypeData\Pscx.SIUnits.Type.ps1xml',
        'TypeData\Pscx.TerminalServices.Type.ps1xml',
        'TypeData\Pscx.Wmi.Type.ps1xml'
    )

    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{

        PSData = @{

            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('PSEdition_Desktop','Windows','Utilities','Xml','Zip','Clipboard','Base64','ShortPath','PEHeader')

            # A URL to the license for this module.
            LicenseUri = 'https://github.com/Pscx/Pscx/blob/master/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/Pscx/Pscx'

            # A URL to an icon representing this module.
            IconUri = 'https://github.com/Pscx/Pscx/blob/master/PscxIcon.png?raw=true'

            Prerelease = 'beta4'

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
