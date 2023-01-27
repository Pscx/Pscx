# ---------------------------------------------------------------------------
# You can override individual preferences by passing a hashtable with just those
# preference defined as shown below:
#
#     Import-Module Pscx -arg @{ModulesToImport = @{FileSystem = $true}}
#
# Any value not specified will be retrieved from the default preferences built
# into the PSCX DLL.
#
# If you have a sufficiently large number of altered preferences, copy this file,
# modify it and pass the path to your preferences file to Import-Module e.g.:
#
#     Import-Module Pscx -arg "$(Split-Path $profile -parent)\Pscx.UserPreferences.ps1"
#
# ---------------------------------------------------------------------------

@{
    ShowModuleLoadDetails = $false    # Display module load details during Import-Module

    CD_GetChildItem = $false          # Display the contents of new provider location after using
                                      # cd (Set-LocationEx).  Mutually exclusive with CD_EchoNewLocation.

    CD_EchoNewLocation = $false       # Display new provider location after using cd (Set-LocationEx).
                                      # Mutually exclusive with CD_GetChildItem.

    TextEditor = 'Notepad.exe'        # Default text editor used by the Edit-File function
                                      # For Visual Studio Code use 'code.cmd'

    PageHelpUsingLess = $true         # Pscx replaces PowerShell's More function. When this setting
                                      # is set to $true, less.exe is used to page items piped
                                      # to the More function. Less.exe is powerful paging app
                                      # that allows advanced navigation and search. Press 'h' to
                                      # access help inside less.exe and 'q' to exit less.exe.
                                      # Set this setting to $false to use more.com for paging.

    FileSizeInUnits = $true          # Pscx prepends format data for display of file information.
                                      # If this value is set to $true, file sizes are displayed in
                                      # using KB,MG,GB and TB units.

    ModulesToImport = @{
        CD                = $true
        DirectoryServices = $false    # provided by submodule PscxWin only on Windows
        FileSystem        = $false    # not of real value
        Net               = $true     # register some format types
        TranscribeSession = $false    # Disabled by default for security and privacy reasons.
        Utility           = $true     # really useful
        Vhd               = $false    # limited need - disabled by default
        Wmi               = $false    # type accelerators only, from PscxWin only on Windows
    }
}