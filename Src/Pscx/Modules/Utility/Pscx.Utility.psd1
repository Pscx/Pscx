@{
    ModuleVersion    = '3.6.4'
    ModuleToProcess  = 'Pscx.Utility.psm1'
    FormatsToProcess = 'Pscx.Utility.Format.ps1xml'
    AliasesToExport = '*'
    FunctionsToExport = @(
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
        'IsViewSelectedByTypeName',
        'GenerateViewDefinition',
        'GenerateViewDefinitions',
        'Stop-RemoteProcess',
        'Get-ScreenCss',
        'Get-ScreenHtml',
        'BuildHtml',
        'OpenElement',
        'CloseElement',
        'Invoke-Method',
        'Set-Writable',
        'Set-FileAttributes',
        'Set-ReadOnly',
        'Show-Tree',
        'GetIndentString',
        'CompactString',
        'ShowItemText',
        'ShowPropertyText',
        'ShowItem',
        'Get-Parameter',
        'Join-Object',
        'Add-Parameters',
        'Import-VisualStudioVars',
        'GetSpecifiedVSSetupInstance',
        'FindAndLoadBatchFile',
        'Get-ExecutionTime',
        'AddRegex'
    )
}
