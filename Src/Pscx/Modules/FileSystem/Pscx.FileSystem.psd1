@{
    ModuleVersion   ="3.6.4"
    ModuleToProcess = 'Pscx.FileSystem.psm1'
    TypesToProcess  = 'Pscx.FileSystem.Type.ps1xml'

	FunctionsToExport = @(
        'Add-DirectoryLength',
		'Add-ShortPath'
	}
}
