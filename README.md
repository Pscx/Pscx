# Pscx - PowerShell Community Extensions

[![Join the chat at https://gitter.im/Pscx/Pscx](https://badges.gitter.im/Pscx/Pscx.svg)](https://gitter.im/Pscx/Pscx?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This PowerShell module is aimed at providing a widely useful set of additional cmdlets, providers, aliases, filters, functions and
scripts for PowerShell that members of the community have expressed interest in.

## Release notes

See [ReleaseNotes.txt](ReleaseNotes.txt).

## Maintainers

 - [Keith Hill](https://github.com/rkeithhill) - [@r_keith_hill](http://twitter.com/r_keith_hill)
 - [Oisin Grehan](https://github.com/oising) - [@oising](http://twitter.com/oising)

### `Add-PathVariable`

Adds the specified paths to the end of the named, path-oriented environment variable.

### `Clear-MSMQueue`

Purges all messages from a queue

### `Convert-Xml`

Performs XSLT transforms on the specified XML file or XmlDocument.

### `ConvertFrom-Base64`

Converts base64 encoded string to byte array.

### `ConvertTo-Base64`

Converts byte array or specified file contents to base64 string.

### `ConvertTo-MacOs9LineEnding`

Converts the line endings in the specified file to Mac OS9 and earlier style line endings "\r".

### `ConvertTo-Metric`

PSCX Cmdlet:

### `ConvertTo-UnixLineEnding`

Converts the line endings in the specified file to Unix line endings "\n".

### `ConvertTo-WindowsLineEnding`

Converts the line endings in the specified file to Windows line endings "\r\n".

### `Disconnect-TerminalSession`

Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop

### `Edit-File`

Edits a file using a regex pattern to find text to be replaced by a specified replacement string.

### `Expand-Archive`

Expands a compressed archive file, or ArchiveEntry object, to its constituent file(s).

### `Export-Bitmap`

Exports bitmap objects to various formats.

### `Format-Byte`

Displays numbers in multiples of byte units.

### `Format-Hex`

System.Object[]

### `Format-Xml`

Pretty print for XML files and XmlDocument objects.

### `Get-ADObject`

Search for objects in the Active Directory/Global Catalog.

### `Get-AdoConnection`

Create an ADO connection to any database supported by .NET on the current machine. You can enumerate available ADO.NET Data Providers with the Get-AdoDataProvider Cmdlet.

### `Get-AdoDataProvider`

List all registered ADO.NET Data Providers on the current machine.

### `Get-Clipboard`

Gets data from the clipboard.

### `Get-DhcpServer`

Gets a list of authorized DHCP servers.

### `Get-DomainController`

Gets domain controllers.

### `Get-DriveInfo`

Gets disk usage information on the system's disk drives.

### `Get-EnvironmentBlock`

Lists the environment blocks stored on the environment block stack.

### `Get-FileTail`

Tails the contents of a file - optionally waiting on new content.

### `Get-FileVersionInfo`

Gets a FileVersionInfo object for the specified path.

### `Get-ForegroundWindow`

Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.

### `Get-Hash`

Gets the hash value for the specified file or byte array via the pipeline.

### `Get-HttpResource`

Gets an HTTP resource or optionally the headers associated with the resource.

### `Get-LoremIpsum`

PSCX Cmdlet:

### `Get-MountPoint`

Returns all mount points defined for a specific root path.

### `Get-MSMQueue`

Returns a list of all queues matching the filter parameters

### `Get-OpticalDriveInfo`

Get information on optical drive capabilities on the local machine.

### `Get-PathVariable`

Gets the specified path-oriented environment variable.

### `Get-PEHeader`

Gets the Portable Header information from an executable file.

### `Get-Privilege`

Lists privileges held by the session and their current status.

### `Get-PSSnapinHelp`

Generates a XML file containing all documentation data.

### `Get-ReparsePoint`

Gets NTFS reparse point data.

### `Get-RunningObject`

PSCX Cmdlet:

### `Get-ShortPath`

Gets the short, 8.3 name for the given path.

### `Get-TerminalSession`

Gets information on terminal services sessions.

### `Get-TypeName`

Get-TypeName displays the typename of the input object.

### `Get-Uptime`

Gets the operating system's uptime and last bootup time.

### `Import-Bitmap`

Loads bitmap files.

### `Invoke-AdoCommand`

Execute a SQL query against an ADO.NET datasource.

### `Invoke-Apartment`

PSCX Cmdlet:

### `Join-String`

Joins an array of strings into a single string.

### `New-Hardlink`

Creates filesystem hard links. The hardlink and the target must reside on the same NTFS volume.

### `New-Junction`

Creates NTFS directory junctions.

### `New-MSMQueue`

Creates a new queue object with the defined properties

### `New-Shortcut`

Creates shell shortcuts.

### `New-Symlink`

Creates filesystem symbolic links. Requires Microsoft Windows Vista or later.

### `Out-Clipboard`

Formats text via Out-String before placing in clipboard. Can also place string in clipboard as a file.

### `Ping-Host`

Sends ICMP echo requests to network hosts.

### `Pop-EnvironmentBlock`

Pops the topmost environment block.

### `Push-EnvironmentBlock`

Pushes the current environment onto the environment block stack.

### `Read-Archive`

Enumerates compressed archives such as 7z or rar, emitting ArchiveEntry objects representing records in the archive.

### `Receive-MSMQueue`

Receives the first message available in the queue. This call is synchronous, and blocks the current thread of execution until a message is available.

### `Remove-MountPoint`

Removes a mount point, dismounting the current media if any. If used against the root of a fixed drive, removes the drive letter assignment.

### `Remove-ReparsePoint`

Removes NTFS reparse junctions and symbolic links.

### `Resolve-Host`

Resolves host names to IP addresses.

### `Send-MSMQueue`

Wraps an object in a Message, and places it onto the defined queue.

### `Send-SmtpMail`

Sends email via specified SMTP server to specified recipients.

### `Set-BitmapSize`

Sets the size of the specified bitmap.

### `Set-Clipboard`

Puts the specified object into the system clipboard.

### `Set-FileTime`

Sets a file or folder's created and last accessed/write times.

### `Set-ForegroundWindow`

Given an hWnd or window handle, brings that window to the foreground. Useful for restoring a window to uppermost after an application which seizes the foreground is invoked. See also Get-ForegroundWindow

### `Set-PathVariable`

Sets the specified path-oriented environment variable.

### `Set-Privilege`

Adjusts privileges associated with a user (identity).

### `Set-VolumeLabel`

Modifies the label shown in Windows Explorer for a particular disk volume.

### `Skip-Object`

Skips the specified objects in the pipeline.

### `Split-String`

Splits a single string into an array of strings.

### `Stop-TerminalSession`

Logs off a specific remote desktop session on a system running Terminal Services/Remote Desktop

### `Test-AlternateDataStream`

Tests for the existence of the specified alternate data stream from an NTFS file.

### `Test-Assembly`

Tests whether or not the specified file is a .NET assembly.

### `Test-MSMQueue`

PSCX Cmdlet:

### `Test-Script`

Determines whether a PowerShell script has any syntax errors.

### `Test-UserGroupMembership`

Tests whether or not a user (current user by default) is a member of the specified group name.

### `Test-Xml`

Tests for well formedness and optionally validates against XML Schema.

### `Write-BZip2`

Create BZIP2 format archive files from pipline or parameter input.

### `Write-Clipboard`

Writes objects to the clipboard using their string representation, bypassing the default PowerShell formatting.

### `Write-GZip`

Create GNU ZIP (GZIP) format files from pipeline or parameter input.

### `Write-Tar`

Create Tape Archive (TAR) format files from pipeline or parameter input.

### `Write-Zip`

Create ZIP format archive files from pipline or parameter input.

### `Add-PathVariable`

Adds the specified paths to the end of the named, path-oriented environment variable.

### `Clear-MSMQueue`

Purges all messages from a queue

### `Convert-Xml`

Performs XSLT transforms on the specified XML file or XmlDocument.

### `ConvertFrom-Base64`

Converts base64 encoded string to byte array.

### `ConvertTo-Base64`

Converts byte array or specified file contents to base64 string.

### `ConvertTo-MacOs9LineEnding`

Converts the line endings in the specified file to Mac OS9 and earlier style line endings "\r".

### `ConvertTo-Metric`

PSCX Cmdlet:

### `ConvertTo-UnixLineEnding`

Converts the line endings in the specified file to Unix line endings "\n".

### `ConvertTo-WindowsLineEnding`

Converts the line endings in the specified file to Windows line endings "\r\n".

### `Disconnect-TerminalSession`

Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop

### `Edit-File`

Edits a file using a regex pattern to find text to be replaced by a specified replacement string.

### `Expand-Archive`

Expands a compressed archive file, or ArchiveEntry object, to its constituent file(s).

### `Export-Bitmap`

Exports bitmap objects to various formats.

### `Format-Byte`

Displays numbers in multiples of byte units.

### `Format-Hex`

System.Object[]

### `Format-Xml`

Pretty print for XML files and XmlDocument objects.

### `Get-ADObject`

Search for objects in the Active Directory/Global Catalog.

### `Get-AdoConnection`

Create an ADO connection to any database supported by .NET on the current machine. You can enumerate available ADO.NET Data Providers with the Get-AdoDataProvider Cmdlet.

### `Get-AdoDataProvider`

List all registered ADO.NET Data Providers on the current machine.

### `Get-Clipboard`

Gets data from the clipboard.

### `Get-DhcpServer`

Gets a list of authorized DHCP servers.

### `Get-DomainController`

Gets domain controllers.

### `Get-DriveInfo`

Gets disk usage information on the system's disk drives.

### `Get-EnvironmentBlock`

Lists the environment blocks stored on the environment block stack.

### `Get-FileTail`

Tails the contents of a file - optionally waiting on new content.

### `Get-FileVersionInfo`

Gets a FileVersionInfo object for the specified path.

### `Get-ForegroundWindow`

Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.

### `Get-Hash`

Gets the hash value for the specified file or byte array via the pipeline.

### `Get-HttpResource`

Gets an HTTP resource or optionally the headers associated with the resource.

### `Get-LoremIpsum`

PSCX Cmdlet:

### `Get-MountPoint`

Returns all mount points defined for a specific root path.

### `Get-MSMQueue`

Returns a list of all queues matching the filter parameters

### `Get-OpticalDriveInfo`

Get information on optical drive capabilities on the local machine.

### `Get-PathVariable`

Gets the specified path-oriented environment variable.

### `Get-PEHeader`

Gets the Portable Header information from an executable file.

### `Get-Privilege`

Lists privileges held by the session and their current status.

### `Get-PSSnapinHelp`

Generates a XML file containing all documentation data.

### `Get-ReparsePoint`

Gets NTFS reparse point data.

### `Get-RunningObject`

PSCX Cmdlet:

### `Get-ShortPath`

Gets the short, 8.3 name for the given path.

### `Get-TerminalSession`

Gets information on terminal services sessions.

### `Get-TypeName`

Get-TypeName displays the typename of the input object.

### `Get-Uptime`

Gets the operating system's uptime and last bootup time.

### `Import-Bitmap`

Loads bitmap files.

### `Invoke-AdoCommand`

Execute a SQL query against an ADO.NET datasource.

### `Invoke-Apartment`

PSCX Cmdlet:

### `Join-String`

Joins an array of strings into a single string.

### `New-Hardlink`

Creates filesystem hard links. The hardlink and the target must reside on the same NTFS volume.

### `New-Junction`

Creates NTFS directory junctions.

### `New-MSMQueue`

Creates a new queue object with the defined properties

### `New-Shortcut`

Creates shell shortcuts.

### `New-Symlink`

Creates filesystem symbolic links. Requires Microsoft Windows Vista or later.

### `Out-Clipboard`

Formats text via Out-String before placing in clipboard. Can also place string in clipboard as a file.

### `Ping-Host`

Sends ICMP echo requests to network hosts.

### `Pop-EnvironmentBlock`

Pops the topmost environment block.

### `Push-EnvironmentBlock`

Pushes the current environment onto the environment block stack.

### `Read-Archive`

Enumerates compressed archives such as 7z or rar, emitting ArchiveEntry objects representing records in the archive.

### `Receive-MSMQueue`

Receives the first message available in the queue. This call is synchronous, and blocks the current thread of execution until a message is available.

### `Remove-MountPoint`

Removes a mount point, dismounting the current media if any. If used against the root of a fixed drive, removes the drive letter assignment.

### `Remove-ReparsePoint`

Removes NTFS reparse junctions and symbolic links.

### `Resolve-Host`

Resolves host names to IP addresses.

### `Send-MSMQueue`

Wraps an object in a Message, and places it onto the defined queue.

### `Send-SmtpMail`

Sends email via specified SMTP server to specified recipients.

### `Set-BitmapSize`

Sets the size of the specified bitmap.

### `Set-Clipboard`

Puts the specified object into the system clipboard.

### `Set-FileTime`

Sets a file or folder's created and last accessed/write times.

### `Set-ForegroundWindow`

Given an hWnd or window handle, brings that window to the foreground. Useful for restoring a window to uppermost after an application which seizes the foreground is invoked. See also Get-ForegroundWindow

### `Set-PathVariable`

Sets the specified path-oriented environment variable.

### `Set-Privilege`

Adjusts privileges associated with a user (identity).

### `Set-VolumeLabel`

Modifies the label shown in Windows Explorer for a particular disk volume.

### `Skip-Object`

Skips the specified objects in the pipeline.

### `Split-String`

Splits a single string into an array of strings.

### `Stop-TerminalSession`

Logs off a specific remote desktop session on a system running Terminal Services/Remote Desktop

### `Test-AlternateDataStream`

Tests for the existence of the specified alternate data stream from an NTFS file.

### `Test-Assembly`

Tests whether or not the specified file is a .NET assembly.

### `Test-MSMQueue`

PSCX Cmdlet:

### `Test-Script`

Determines whether a PowerShell script has any syntax errors.

### `Test-UserGroupMembership`

Tests whether or not a user (current user by default) is a member of the specified group name.

### `Test-Xml`

Tests for well formedness and optionally validates against XML Schema.

### `Write-BZip2`

Create BZIP2 format archive files from pipline or parameter input.

### `Write-Clipboard`

Writes objects to the clipboard using their string representation, bypassing the default PowerShell formatting.

### `Write-GZip`

Create GNU ZIP (GZIP) format files from pipeline or parameter input.

### `Write-Tar`

Create Tape Archive (TAR) format files from pipeline or parameter input.

### `Write-Zip`

Create ZIP format archive files from pipline or parameter input.

### `Add-DirectoryLength`

Calculates the sizes of the specified directory and adds that size
as a "Length" NoteProperty to the input DirectoryInfo object.

### `Add-ShortPath`

Adds the file or directory's short path as a "ShortPath" NoteProperty to each input object.

### `Dismount-VHD`

Dismounts a Virtual Hard Drive (VHD) file.

### `Edit-HostProfile`

Opens the current user's profile for the current host in a text editor.

### `Edit-Profile`

Opens the current user's "all hosts" profile in a text editor.

### `Enable-OpenPowerShellHere`

Creates the registry entries required to create Windows Explorer context
menu "Open PowerShell Here" for both Directories and Drives

### `Get-ExecutionTime`

Gets the execution time for the specified Id of a command in the current
session history.

### `Get-Parameter`

Enumerates the parameters of one or more commands.

### `Get-ScreenCss`

Generate CSS header for HTML "screen shot" of the host buffer.

### `Get-ScreenHtml`

Functions to generate HTML "screen shot" of the host buffer.

### `Get-ViewDefinition`

Gets the possible alternate views for the specified object.

### `help`

Displays information about Windows PowerShell commands and concepts.

### `Import-VisualStudioVars`

Imports environment variables for the specified version of Visual Studio.

### `Invoke-BatchFile`

Invokes the specified batch file and retains any environment variable changes it makes.

### `Invoke-Elevated`

Runs the specified command in an elevated context.

### `Invoke-GC`

Invokes the .NET garbage collector to clean up garbage objects.

### `Invoke-Method`

Function to call a single method on an incoming stream of piped objects.

### `Invoke-NullCoalescing`

Similar to the C# ?? operator e.g. name = value ?? String.Empty

### `Invoke-Ternary`

Similar to the C# ? : operator e.g. name = (value != null) ? String.Empty : value

### `less`

Less provides better paging of output from cmdlets.

### `Mount-VHD`

Mounts a Virtual Hard Drive (VHD) file.

### `New-HashObject`

Create a PSObject from a dictionary such as a hashtable.

### `Out-Speech`

Outputs text as spoken words.

### `QuoteList`

Convenience function for creating an array of strings without requiring quotes or commas.

### `QuoteString`

Creates a string from each parameter by concatenating each item using $OFS as the separator.

### `Resolve-ErrorRecord`

Resolves the PowerShell error code to a textual description of the error.

### `Resolve-HResult`

Resolves the hresult error code to a textual description of the error.

### `Resolve-WindowsError`

Resolves a Windows error number a textual description of the error.

### `Set-LocationEx`

CD function that tracks location history allowing easy navigation to previous locations.

### `Set-ReadOnly`

Sets a file's read only status to true making it read only.

### `Set-Writable`

Sets a file's read only status to false making it writable.

### `Show-Tree`

Shows the specified path as a tree.

### `Start-PowerShell`

Starts a new Windows PowerShell process.

### `Stop-RemoteProcess`

Stops a process on a remote machine.

### `?:`

System.Object[]

### `??`

System.Object[]

### `call`

Function to call a single method on an incoming stream of piped objects.

### `cvxml`

Performs XSLT transforms on the specified XML file or XmlDocument.

### `e`

Edits a file using a regex pattern to find text to be replaced by a specified replacement string.

### `ehp`

Opens the current user's profile for the current host in a text editor.

### `ep`

Opens the current user's "all hosts" profile in a text editor.

### `fhex`


Format-Hex [-Path] <string[]> [<CommonParameters>]

Format-Hex -LiteralPath <string[]> [<CommonParameters>]

Format-Hex -InputObject <Object> [-Encoding <string>] [-Raw] [<CommonParameters>]


### `fxml`

Pretty print for XML files and XmlDocument objects.

### `gcb`

Gets data from the clipboard.

### `gpar`

Enumerates the parameters of one or more commands.

### `gtn`

Get-TypeName displays the typename of the input object.

### `igc`

Invokes the .NET garbage collector to clean up garbage objects.

### `ln`

Creates filesystem hard links. The hardlink and the target must reside on the same NTFS volume.

### `lorem`

PSCX Cmdlet:

### `nho`

Create a PSObject from a dictionary such as a hashtable.

### `ocb`

Formats text via Out-String before placing in clipboard. Can also place string in clipboard as a file.

### `ql`

Convenience function for creating an array of strings without requiring quotes or commas.

### `qs`

Creates a string from each parameter by concatenating each item using $OFS as the separator.

### `Resize-Bitmap`

Sets the size of the specified bitmap.

### `rver`

Resolves the PowerShell error code to a textual description of the error.

### `rvhr`

Resolves the hresult error code to a textual description of the error.

### `rvwer`

Resolves a Windows error number a textual description of the error.

### `skip`

Skips the specified objects in the pipeline.

### `sro`

Sets a file's read only status to true making it read only.

### `su`

Runs the specified command in an elevated context.

### `swr`

Sets a file's read only status to false making it writable.

### `tail`

Tails the contents of a file - optionally waiting on new content.

### `touch`

Sets a file or folder's created and last accessed/write times.

