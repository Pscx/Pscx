@{
    GUID               = '0d3e3058-faa0-4378-9834-45fb0e496ec8'
    Author             = 'PowerShell Core Community Extensions Team'
    CompanyName        = 'PowerShell Core Community Extensions'
    Copyright          = 'Copyright PowerShell Core Community Extensions Team 2006 - 2023.'
    Description        = 'PowerShell Core Community Extensions (PSCX) module which implements Windows OS specific set of Cmdlets.'
    PowerShellVersion  = '7.2'
    CLRVersion         = '6.0'
    ModuleVersion      = '3.6.4'
    RequiredAssemblies = 'Pscx.Win.dll'
    RootModule         = 'PscxWin.psm1'
    NestedModules      = 'Pscx.Win.dll'
    AliasesToExport    = '*'
    CmdletsToExport    = @(
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
        'Resolve-HResult',
        'Resolve-WindowsError'
    )
    FormatsToProcess   = @(
        'FormatData\Pscx.Archive.Format.ps1xml',
        'FormatData\Pscx.Security.Format.ps1xml',
        'FormatData\Pscx.TerminalServices.Format.ps1xml'
    )
    TypesToProcess     = @(
        'TypeData\Pscx.Archive.Type.ps1xml',
        'TypeData\Pscx.TerminalServices.Type.ps1xml',
        'TypeData\Pscx.Wmi.Type.ps1xml'
    )

    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{

        PSData = @{

            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('Utilities','Windows','PSCX','ActiveDirectory','WMI')

            # A URL to the license for this module.
            LicenseUri = 'https://gitlab.com/danluca/pscx-light/-/blob/master/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://gitlab.com/danluca/pscx-light'

            # A URL to an icon representing this module.
            IconUri = 'https://gitlab.com/danluca/pscx-light/-/blob/master/PscxIcon.png?raw=true'

            # Release notes
            ReleaseNotes = @'
3.5.0 - September 20, 2021

* Extracted Windows specific commands and cmdlets
'@
        } # End of PSData hashtable
    } # End of PrivateData hashtable
}
