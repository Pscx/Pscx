# Pscx - PowerShell Community Extensions

This PowerShell module is aimed at providing a widely useful set of additional cmdlets, providers, aliases, filters,
functions and scripts for PowerShell that members of the community have expressed interest in.

## Release notes

See [ReleaseNotes.txt](ReleaseNotes.txt).

## Install Pscx

Pscx is hosted on the PowerShell Gallery.  You can install Pscx with the following command:

```powershell
Install-Module Pscx -Scope CurrentUser
```

You may be prompted to trust the PSGallery.  Respond with a 'y' (for Yes) or 'A' (for All) to proceed with the install.

This extension replaces a number of PowerShell commands that are installed by other extensions.
If this is the case the command emits this warning:

```powershell
The following commands are already available on this system:
'gcb, Expand-Archive, Format-Hex, Get-Hash, help, prompt, Get-Clipboard, Get-Help, Set-Clipboard'. This module 'Pscx'
may override the existing commands. If you still want to install this module 'Pscx', use -AllowClobber parameter.
```
To force the installation use this command:

```powershell
Install-Module Pscx -AllowClobber -Scope CurrentUser
```

There may be further issues to overcome.
Powershell security policy may block the 'Pscx' module from loading:

```powershell
New-SymLink D:\ parts_library
New-SymLink : The 'New-SymLink' command was found in the module 'Pscx', but the module could not be loaded. For more
information, run 'Import-Module Pscx'.
```

Running the suggested command may provide this feedback:

```powershell
PS C:\> Import-Module Pscx
Import-Module : File C:\WindowsPowerShell\Modules\Pscx\3.3.2\Pscx.psm1 cannot be loaded
because running scripts is disabled on this system. For more information, see 
[about_Execution_Policies at](https:/go.microsoft.com/fwlink/?LinkID=135170).
```

Using the information on that page this command is suggested:

```powershell
PS C:> Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy AllSigned
```

Which permits the command Import-Module Pscx to run and thus make the Pscx commands available.

If you already have installed Pscx from the PowerShell Gallery, you can update Pscx with the command:

```powershell
Update-Module Pscx
```

## Maintainers

- [Keith Hill](https://github.com/rkeithhill) - [@r_keith_hill](http://twitter.com/r_keith_hill)
- [Oisin Grehan](https://github.com/oising) - [@oising](http://twitter.com/oising)

## Included cmdlets and functions

Cmdlets and functions below are sorted by noun. As always, you can get full Powershell help including examples using `get-help [command]`

### ADObject

#### `Get-ADObject`

Search for objects in the Active Directory/Global Catalog.

### AdoCommand

#### `Invoke-AdoCommand`

Execute a SQL query against an ADO.NET datasource.

### AdoConnection

#### `Get-AdoConnection`

Create an ADO connection to any database supported by .NET on the current machine. You can enumerate available ADO.NET Data Providers with the Get-AdoDataProvider Cmdlet.

### AdoDataProvider

#### `Get-AdoDataProvider`

List all registered ADO.NET Data Providers on the current machine.

### AlternateDataStream

#### `Test-AlternateDataStream`

Tests for the existence of the specified alternate data stream from an NTFS file.

### Apartment

#### `Invoke-Apartment`

PSCX Cmdlet:

### PscxArchive

#### `Expand-PscxArchive`

Expands a compressed archive file, or ArchiveEntry object, to its constituent file(s).

#### `Read-PscxArchive`

Enumerates compressed archives such as 7z or rar, emitting ArchiveEntry objects representing records in the archive.

### Assembly

#### `Test-Assembly`

Tests whether or not the specified file is a .NET assembly.

### Base64

#### `ConvertFrom-Base64`

Converts base64 encoded string to byte array.

#### `ConvertTo-Base64`

Converts byte array or specified file contents to base64 string.

### BatchFile

#### `Invoke-BatchFile`

Invokes the specified batch file and retains any environment variable changes it makes.

### Bitmap

#### `Export-Bitmap`

Exports bitmap objects to various formats.

#### `Import-Bitmap`

Loads bitmap files.

### BitmapSize

#### `Set-BitmapSize`

Sets the size of the specified bitmap.

### Byte

#### `Format-Byte`

Displays numbers in multiples of byte units.

### BZip2

#### `Write-BZip2`

Create BZIP2 format archive files from pipeline or parameter input.

### PscxClipboard

#### `Get-PscxClipboard`

Gets data from the clipboard.

#### `Out-PscxClipboard`

Formats text via Out-String before placing in clipboard. Can also place string in clipboard as a file.

#### `Set-PscxClipboard`

Puts the specified object into the system clipboard.

#### `Write-PscxClipboard`

Writes objects to the clipboard using their string representation, bypassing the default PowerShell formatting.

### DhcpServer

#### `Get-DhcpServer`

Gets a list of authorized DHCP servers.

### DirectoryLength

#### `Add-DirectoryLength`

Calculates the sizes of the specified directory and adds that size
as a "Length" NoteProperty to the input DirectoryInfo object.

### DomainController

#### `Get-DomainController`

Gets domain controllers.

### DriveInfo

#### `Get-DriveInfo`

Gets disk usage information on the system's disk drives.

### Elevated

#### `Invoke-Elevated`

Runs the specified command in an elevated context.

### EnvironmentBlock

#### `Get-EnvironmentBlock`

Lists the environment blocks stored on the environment block stack.

#### `Pop-EnvironmentBlock`

Pops the topmost environment block.

#### `Push-EnvironmentBlock`

Pushes the current environment onto the environment block stack.

### ErrorRecord

#### `Resolve-ErrorRecord`

Resolves the PowerShell error code to a textual description of the error.

### ExecutionTime

#### `Get-ExecutionTime`

Gets the execution time for the specified Id of a command in the current
session history.

### File

#### `Edit-File`

Edits a file using a regex pattern to find text to be replaced by a specified replacement string.

### FileTail

#### `Get-FileTail`

Tails the contents of a file - optionally waiting on new content.

### FileTime

#### `Set-FileTime`

Sets a file or folder's created and last accessed/write times.

### FileVersionInfo

#### `Get-FileVersionInfo`

Gets a FileVersionInfo object for the specified path.

### ForegroundWindow

#### `Get-ForegroundWindow`

Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.

#### `Set-ForegroundWindow`

Given an hWnd or window handle, brings that window to the foreground. Useful for restoring a window to uppermost after an application which seizes the foreground is invoked. See also Get-ForegroundWindow

### GC

#### `Invoke-GC`

Invokes the .NET garbage collector to clean up garbage objects.

### GZip

#### `Write-GZip`

Create GNU ZIP (GZIP) format files from pipeline or parameter input.

### Hardlink

#### `New-Hardlink`

Creates filesystem hard links. The hardlink and the target must reside on the same NTFS volume.

### Hash

#### `Get-Hash`

Gets the hash value for the specified file or byte array via the pipeline.

### HashObject

#### `New-HashObject`

Create a PSObject from a dictionary such as a hashtable.

### PscxHex

#### `Format-PsxHex`

System.Object[]

### Host

#### `Ping-Host`

Sends ICMP echo requests to network hosts.

#### `Resolve-Host`

Resolves host names to IP addresses.

### HostProfile

#### `Edit-HostProfile`

Opens the current user's profile for the current host in a text editor.

### HResult

#### `Resolve-HResult`

Resolves the hresult error code to a textual description of the error.

### HttpResource

#### `Get-HttpResource`

Gets an HTTP resource or optionally the headers associated with the resource.

### Junction

#### `New-Junction`

Creates NTFS directory junctions.

### PscxLocation

#### `Set-PscxLocation`

CD function that tracks location history allowing easy navigation to previous locations.

### LoremIpsum

#### `Get-LoremIpsum`

PSCX Cmdlet:

### MacOs9LineEnding

#### `ConvertTo-MacOs9LineEnding`

Converts the line endings in the specified file to Mac OS9 and earlier style line endings "\r".

### Method

#### `Invoke-Method`

Function to call a single method on an incoming stream of piped objects.

### Metric

#### `ConvertTo-Metric`

PSCX Cmdlet:

### MountPoint

#### `Get-MountPoint`

Returns all mount points defined for a specific root path.

#### `Remove-MountPoint`

Removes a mount point, dismounting the current media if any. If used against the root of a fixed drive, removes the drive letter assignment.

### MSMQueue

#### `Clear-MSMQueue`

Purges all messages from a queue

#### `Get-MSMQueue`

Returns a list of all queues matching the filter parameters

#### `New-MSMQueue`

Creates a new queue object with the defined properties

#### `Receive-MSMQueue`

Receives the first message available in the queue. This call is synchronous, and blocks the current thread of execution until a message is available.

#### `Send-MSMQueue`

Wraps an object in a Message, and places it onto the defined queue.

#### `Test-MSMQueue`

PSCX Cmdlet:

### NullCoalescing

#### `Invoke-NullCoalescing`

Similar to the C# ?? operator e.g. name = value ?? String.Empty

### Object

#### `Skip-Object`

Skips the specified objects in the pipeline.

### OpenPowerShellHere

#### `Enable-OpenPowerShellHere`

Creates the registry entries required to create Windows Explorer context
menu "Open PowerShell Here" for both Directories and Drives

### OpticalDriveInfo

#### `Get-OpticalDriveInfo`

Get information on optical drive capabilities on the local machine.

### Parameter

#### `Get-Parameter`

Enumerates the parameters of one or more commands.

### PathVariable

#### `Add-PathVariable`

Adds the specified paths to the end of the named, path-oriented environment variable.

#### `Get-PathVariable`

Gets the specified path-oriented environment variable.

#### `Set-PathVariable`

Sets the specified path-oriented environment variable.

### PEHeader

#### `Get-PEHeader`

Gets the Portable Header information from an executable file.

### PowerShell

#### `Start-PowerShell`

Starts a new Windows PowerShell process.

### Privilege

#### `Get-Privilege`

Lists privileges held by the session and their current status.

#### `Set-Privilege`

Adjusts privileges associated with a user (identity).

### Profile

#### `Edit-Profile`

Opens the current user's "all hosts" profile in a text editor.

### PSSnapinHelp

#### `Get-PSSnapinHelp`

Generates a XML file containing all documentation data.

### ReadOnly

#### `Set-ReadOnly`

Sets a file's read only status to true making it read only.

### RemoteProcess

#### `Stop-RemoteProcess`

Stops a process on a remote machine.

### ReparsePoint

#### `Get-ReparsePoint`

Gets NTFS reparse point data.

#### `Remove-ReparsePoint`

Removes NTFS reparse junctions and symbolic links.

### RunningObject

#### `Get-RunningObject`

PSCX Cmdlet:

### ScreenCss

#### `Get-ScreenCss`

Generate CSS header for HTML "screen shot" of the host buffer.

### ScreenHtml

#### `Get-ScreenHtml`

Functions to generate HTML "screen shot" of the host buffer.

### Script

#### `Test-Script`

Determines whether a PowerShell script has any syntax errors.

### Shortcut

#### `New-Shortcut`

Creates shell shortcuts.

### ShortPath

#### `Add-ShortPath`

Adds the file or directory's short path as a "ShortPath" NoteProperty to each input object.

#### `Get-ShortPath`

Gets the short, 8.3 name for the given path.

### SmtpMail

#### `Send-SmtpMail`

Sends email via specified SMTP server to specified recipients.

### Speech

#### `Out-Speech`

Outputs text as spoken words.

### String

#### `Join-PscxString`

Joins an array of strings into a single string.

#### `Split-String`

Splits a single string into an array of strings.

### Symlink

#### `New-Symlink`

Creates filesystem symbolic links. Requires Microsoft Windows Vista or later.

### Tar

#### `Write-Tar`

Create a Tape ARchive (TAR) format file from a pipeline or parameter input.

### TerminalSession

#### `Disconnect-TerminalSession`

Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop

#### `Get-TerminalSession`

Gets information on terminal services sessions.

#### `Stop-TerminalSession`

Logs off a specific remote desktop session on a system running Terminal Services/Remote Desktop

### Ternary

#### `Invoke-Ternary`

Similar to the C# ? : operator e.g. name = (value != null) ? String.Empty : value

### Tree

#### `Show-Tree`

Shows the specified path as a tree.

### TypeName

#### `Get-TypeName`

Get-TypeName displays the typename of the input object.

### UnixLineEnding

#### `ConvertTo-UnixLineEnding`

Converts the line endings in the specified file to Unix line endings "\n".

### PscxUptime

#### `Get-PscxUptime`

Gets the operating system's uptime and last bootup time.

### UserGroupMembership

#### `Test-UserGroupMembership`

Tests whether or not a user (current user by default) is a member of the specified group name.

### VHD

#### `Dismount-VHD`

Dismounts a Virtual Hard Drive (VHD) file.

#### `Mount-VHD`

Mounts a Virtual Hard Drive (VHD) file.

### ViewDefinition

#### `Get-ViewDefinition`

Gets the possible alternate views for the specified object.

### VisualStudioVars

#### `Import-VisualStudioVars`

Imports environment variables for the specified version of Visual Studio.

### VolumeLabel

#### `Set-VolumeLabel`

Modifies the label shown in Windows Explorer for a particular disk volume.

### WindowsError

#### `Resolve-WindowsError`

Resolves a Windows error number a textual description of the error.

### WindowsLineEnding

#### `ConvertTo-WindowsLineEnding`

Converts the line endings in the specified file to Windows line endings "\r\n".

### Writable

#### `Set-Writable`

Sets a file's read only status to false making it writable.

### Xml

#### `Convert-Xml`

Performs XSLT transforms on the specified XML file or XmlDocument.

#### `Format-Xml`

Pretty print for XML files and XmlDocument objects.

#### `Test-Xml`

Tests for well formedness and optionally validates against XML Schema.

### Zip

#### `Write-Zip`

Create ZIP format archive files from pipeline or parameter input.
