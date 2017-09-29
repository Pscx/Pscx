# Pscx - PowerShell Community Extensions

[![Join the chat at https://gitter.im/Pscx/Pscx](https://badges.gitter.im/Pscx/Pscx.svg)](https://gitter.im/Pscx/Pscx?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This PowerShell module is aimed at providing a widely useful set of additional cmdlets, providers, aliases, filters, functions and
scripts for PowerShell that members of the community have expressed interest in.

## Release notes

See [ReleaseNotes.txt](ReleaseNotes.txt).

## Maintainers

 - [Keith Hill](https://github.com/rkeithhill) - [@r_keith_hill](http://twitter.com/r_keith_hill)
 - [Oisin Grehan](https://github.com/oising) - [@oising](http://twitter.com/oising)

## Included cmdlets and functions

Cmdlets and functions below are sorted by noun. As always, you can get full Powershell help including examples using `get-help [command]`

### ADObject

#### `Get-ADObject`

Search for objects in the Active Directory/Global Catalog.

### AdoConnection

#### `Get-AdoConnection`

Create an ADO connection to any database supported by .NET on the current machine. You can enumerate available ADO.NET Data Providers with the Get-AdoDataProvider Cmdlet.

### AdoDataProvider

#### `Get-AdoDataProvider`

List all registered ADO.NET Data Providers on the current machine.

### Archive

#### `Expand-Archive`

Expands a compressed archive file, or ArchiveEntry object, to its constituent file(s).

### Base64

#### `ConvertFrom-Base64`

Converts base64 encoded string to byte array.

#### `ConvertTo-Base64`

Converts byte array or specified file contents to base64 string.

### Bitmap

#### `Export-Bitmap`

Exports bitmap objects to various formats.

### Byte

#### `Format-Byte`

Displays numbers in multiples of byte units.

### Clipboard

#### `Get-Clipboard`

Gets data from the clipboard.

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

### EnvironmentBlock

#### `Get-EnvironmentBlock`

Lists the environment blocks stored on the environment block stack.

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

### FileVersionInfo

#### `Get-FileVersionInfo`

Gets a FileVersionInfo object for the specified path.

### ForegroundWindow

#### `Get-ForegroundWindow`

Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.

### Hash

#### `Get-Hash`

Gets the hash value for the specified file or byte array via the pipeline.

### Hex

#### `Format-Hex`

System.Object[]

### HostProfile

#### `Edit-HostProfile`

Opens the current user's profile for the current host in a text editor.

### HttpResource

#### `Get-HttpResource`

Gets an HTTP resource or optionally the headers associated with the resource.

### LoremIpsum

#### `Get-LoremIpsum`

PSCX Cmdlet:

### MacOs9LineEnding

#### `ConvertTo-MacOs9LineEnding`

Converts the line endings in the specified file to Mac OS9 and earlier style line endings "\r".

### Metric

#### `ConvertTo-Metric`

PSCX Cmdlet:

### MountPoint

#### `Get-MountPoint`

Returns all mount points defined for a specific root path.

### MSMQueue

#### `Clear-MSMQueue`

Purges all messages from a queue

#### `Get-MSMQueue`

Returns a list of all queues matching the filter parameters

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

### PEHeader

#### `Get-PEHeader`

Gets the Portable Header information from an executable file.

### Privilege

#### `Get-Privilege`

Lists privileges held by the session and their current status.

### Profile

#### `Edit-Profile`

Opens the current user's "all hosts" profile in a text editor.

### PSSnapinHelp

#### `Get-PSSnapinHelp`

Generates a XML file containing all documentation data.

### ReparsePoint

#### `Get-ReparsePoint`

Gets NTFS reparse point data.

### RunningObject

#### `Get-RunningObject`

PSCX Cmdlet:

### ScreenCss

#### `Get-ScreenCss`

Generate CSS header for HTML "screen shot" of the host buffer.

### ScreenHtml

#### `Get-ScreenHtml`

Functions to generate HTML "screen shot" of the host buffer.

### ShortPath

#### `Add-ShortPath`

Adds the file or directory's short path as a "ShortPath" NoteProperty to each input object.

#### `Get-ShortPath`

Gets the short, 8.3 name for the given path.

### TerminalSession

#### `Disconnect-TerminalSession`

Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop

#### `Get-TerminalSession`

Gets information on terminal services sessions.

### TypeName

#### `Get-TypeName`

Get-TypeName displays the typename of the input object.

### UnixLineEnding

#### `ConvertTo-UnixLineEnding`

Converts the line endings in the specified file to Unix line endings "\n".

### Uptime

#### `Get-Uptime`

Gets the operating system's uptime and last bootup time.

### VHD

#### `Dismount-VHD`

Dismounts a Virtual Hard Drive (VHD) file.

### ViewDefinition

#### `Get-ViewDefinition`

Gets the possible alternate views for the specified object.

### WindowsLineEnding

#### `ConvertTo-WindowsLineEnding`

Converts the line endings in the specified file to Windows line endings "\r\n".

### Xml

#### `Convert-Xml`

Performs XSLT transforms on the specified XML file or XmlDocument.

#### `Format-Xml`

Pretty print for XML files and XmlDocument objects.


