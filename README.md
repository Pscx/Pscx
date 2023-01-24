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

## Add-PathVariable
Adds values to an environment variable of type PATH (default is PATH variable)

## ConvertTo-UnixLineEnding
Converts the line endings in the specified file to Unix line endings \"\\n\".

## ConvertTo-Base64
Converts byte array to base64 string.

## ConvertTo-MacOs9LineEnding
Converts the line endings in the specified file to Mac OS9 and earlier style line endings \"\\r\".

## Set-ForegroundWindow
Given an hWnd or window handle, brings that window to the foreground. Useful for restoring a window to uppermost after an application which seizes the foreground is invoked. See also Get-ForegroundWindow

## Test-Assembly
Tests whether or not the specified file is a .NET assembly.

## Join-PscxString
Joins an array of strings into a single string.

## Convert-Xml
Converts XML through a XSL

## ConvertFrom-Base64
Converts base64 encoded string to byte array.

## Get-ForegroundWindow
Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.

## Set-FileTime
Sets a file or folder's created and last accessed/write times.

## Test-Xml
Tests for well formedness and optionally validates against XML Schema.

## Format-Hex
Displays contents of files for byte streams in hex.

## Get-Hash
Gets the hash value for the specified file or byte array via the pipeline.

## Split-PscxString
Splits a single string into an array of strings.

## ConvertTo-WindowsLineEnding
Converts the line endings in the specified file to Windows line endings \"\\r\\n\".

## Get-FileTail
Tails the contents of a file - optionally waiting on new content.

## Format-Xml
Pretty print for XML files and XmlDocument objects.

## Cmdlets available on Windows OS only
