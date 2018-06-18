@{
    GUID               = '0fab0d39-2f29-4e79-ab9a-fd750c66e6c5'
    Author             = 'PowerShell Community Developers'
    CompanyName        = 'http://pscx.codeplex.com/'
    Copyright          = 'Copyright PowerShell Community Developers 2006 - 2018.'
    Description        = 'PowerShell Community Extensions (PSCX) base module which implements a general purpose set of Cmdlets.'
    PowerShellVersion  = '3.0'
    CLRVersion         = '4.0'
    ModuleVersion      = '3.3.2'
    RequiredAssemblies = 'Pscx.dll' # needed for [pscxmodules] type (does not import cmdlets/providers)
    RootModule         = 'Pscx.psm1'
    NestedModules      = 'Pscx.dll'
    AliasesToExport    = '*'
    CmdletsToExport    = @(
        'Add-PathVariable',
        'Clear-MSMQueue',
        'ConvertFrom-Base64',
        'ConvertTo-Base64',
        'ConvertTo-MacOs9LineEnding',
        'ConvertTo-Metric',
        'ConvertTo-UnixLineEnding',
        'ConvertTo-WindowsLineEnding',
        'Convert-Xml',
        'Disconnect-TerminalSession',
        'Edit-File',
        'Expand-Archive',
        'Export-Bitmap',
        'Format-Byte',
        'Format-Hex',
        'Format-Xml',
        'Get-ADObject',
        'Get-AdoConnection',
        'Get-AdoDataProvider',
        'Get-Clipboard',
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
        'Get-MSMQueue',
        'Get-OpticalDriveInfo',
        'Get-PathVariable',
        'Get-PEHeader',
        'Get-Privilege',
        'Get-PSSnapinHelp',
        'Get-ReparsePoint',
        'Get-RunningObject',
        'Get-ShortPath',
        'Get-TerminalSession',
        'Get-TypeName',
        'Get-Uptime',
        'Import-Bitmap',
        'Invoke-AdoCommand',
        'Invoke-Apartment',
        'Join-String',
        'New-Hardlink',
        'New-Junction',
        'New-MSMQueue',
        'New-Shortcut',
        'New-Symlink',
        'Out-Clipboard',
        'Ping-Host',
        'Pop-EnvironmentBlock',
        'Push-EnvironmentBlock',
        'Read-Archive',
        'Receive-MSMQueue',
        'Remove-MountPoint',
        'Remove-ReparsePoint',
        'Resolve-Host',
        'Send-MSMQueue',
        'Send-SmtpMail',
        'Set-BitmapSize',
        'Set-Clipboard',
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
        'Test-MSMQueue',
        'Test-Script',
        'Test-UserGroupMembership',
        'Test-Xml',
        'Write-BZip2',
        'Write-Clipboard',
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
        'Get-Help',
        'Get-Parameter',
        'Get-PropertyValue',
        'Get-ScreenCss',
        'Get-ScreenHtml',
        'Get-ViewDefinition',
        'help',
        'Import-VisualStudioVars',
        'Invoke-BatchFile',
        'Invoke-Elevated',
        'Invoke-GC',
        'Invoke-Method',
        'Invoke-NullCoalescing',
        'Invoke-Ternary',
        'less',
        'New-HashObject',
        'Out-Speech',
        'prompt',
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
        'Set-LocationEx',
        'Dismount-VHD',
        'Mount-VHD'
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
            Tags = @('Utilities','Xml','Zip','Clipboard','Base64','ShortPath','PEHeader','CD')

            # A URL to the license for this module.
            LicenseUri = 'https://github.com/Pscx/Pscx/blob/master/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/Pscx/Pscx'

            # A URL to an icon representing this module.
            IconUri = 'https://github.com/Pscx/Pscx/blob/master/PscxIcon.png?raw=true'

            # Release notes
            ReleaseNotes = @'
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
