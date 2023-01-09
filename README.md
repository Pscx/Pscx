# Pscx - PowerShell Community Extensions Light

This PowerShell module is aimed at providing a widely useful set of additional cmdlets, providers, aliases, filters, functions and
scripts for PowerShell Core that members of the community have expressed interest in.

This repository is a fork of the official PowerShell Community Extensions hosted and [maintained](https://github.com/Pscx/Pscx#maintainers) at GitHub. The fork has been made from 
version [3.3.2](https://github.com/Pscx/Pscx/releases/tag/v3.3.2) (commit [81b76cf](https://github.com/Pscx/Pscx/commit/81b76cfdb1343f84880e0e2cd647db5c56cf354b)).

The customizations made in this fork include:
* upgrade to PowerShell Core 7.x, .Net (Core) 5.0
* compatibility with MacOS and other *nix OS
* build upgraded to VS2019
* packaging and build improvements throughout
* removed cmdlets with low value, seldomly used

## Release notes

See [Changelog.md](CHANGELOG.md).

## Install Pscx

### Pre-requisites

* Install [latest PowerShell Core version](https://github.com/PowerShell/PowerShell/releases/latest)
* Create a profile - may also want to have a look at [ompgit](https://gitlab.com/danluca/ohmyposhgit) profile enhancer

### Installation

* Download the [latest artifact](https://gitlab.com/danluca/pscx-light/releases/latest) from Package Registry
* Unzip to `~/Documents/PowerShell/Modules` folder
* Import module PSCX in your PowerShell profile file `import-module pscx`

## Maintainers
 - @danluca and other maintainers in this GitLab repository


## Included cmdlets and functions

Cmdlets and functions below are sorted by noun. As always, you can get full Powershell help including examples using `get-help [command]`

### ADObject

#### `Get-ADObject`

Search for objects in the Active Directory/Global Catalog.

### Archive

#### `Expand-Archive`

Expands a compressed archive file, or ArchiveEntry object, to its constituent file(s).

#### `Read-Archive`

Enumerates compressed archives such as 7z or rar, emitting ArchiveEntry objects representing records in the archive.

### Base64

#### `ConvertFrom-Base64`

Converts base64 encoded string to byte array.

#### `ConvertTo-Base64`

Converts byte array or specified file contents to base64 string.

### BatchFile

#### `Invoke-BatchFile`

### Byte

#### `Format-Byte`

Displays numbers in multiples of byte units.

### BZip2

#### `Write-BZip2`

Create BZIP2 format archive files from pipline or parameter input.

### Clipboard

#### `Get-Clipboard`

Gets data from the clipboard.

#### `Out-Clipboard`

Formats text via Out-String before placing in clipboard. Can also place string in clipboard as a file.

#### `Set-Clipboard`

Puts the specified object into the system clipboard.

#### `Write-Clipboard`

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

### Hex

#### `Format-Hex`

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

### LocationEx

#### `Set-LocationEx`

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

### NullCoalescing

#### `Invoke-NullCoalescing`

Similar to the C# ?? operator e.g. name = value ?? String.Empty

### Object

#### `Skip-Object`

Skips the specified objects in the pipeline.

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

### Privilege

#### `Get-Privilege`

Lists privileges held by the session and their current status.

#### `Set-Privilege`

Adjusts privileges associated with a user (identity).

### Profile

#### `Edit-Profile`

Opens the current user's "all hosts" profile in a text editor.

### ReadOnly

#### `Set-ReadOnly`

Sets a file's read only status to true making it read only.

### ReparsePoint

#### `Get-ReparsePoint`

Gets NTFS reparse point data.

#### `Remove-ReparsePoint`

Removes NTFS reparse junctions and symbolic links.

### RunningObject

#### `Get-RunningObject`

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

### String

#### `Join-String`

Joins an array of strings into a single string.

#### `Split-String`

Splits a single string into an array of strings.

### Symlink

#### `New-Symlink`

Creates filesystem symbolic links. Requires Microsoft Windows Vista or later.

### Tar

#### `Write-Tar`

Create Tape Archive (TAR) format files from pipeline or parameter input.

### TerminalSession

#### `Disconnect-TerminalSession`

Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop

#### `Get-TerminalSession`

Gets information on terminal services sessions.

#### `Stop-TerminalSession`

Logs off a specific remote desktop session on a system running Terminal Services/Remote Desktop

### Tree

#### `Show-Tree`

Shows the specified path as a tree.

### TypeName

#### `Get-TypeName`

Get-TypeName displays the typename of the input object.

### UnixLineEnding

#### `ConvertTo-UnixLineEnding`

Converts the line endings in the specified file to Unix line endings "\n".

### Uptime

#### `Get-Uptime`

Gets the operating system's uptime and last bootup time.

### UserGroupMembership

#### `Test-UserGroupMembership`

Tests whether or not a user (current user by default) is a member of the specified group name.

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

Create ZIP format archive files from pipline or parameter input.
