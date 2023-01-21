# SevenZipSharp Library

This is a wrapper library for managing interactions with 7-zip DLL (7z.dll) in C# (low-level 7-zip DLL uses COM based calls). 
From the [project description](https://github.com/squid-box/SevenZipSharp) - _"Managed 7-zip library written in C# that provides data (self-)extraction and compression (all 7-zip formats are supported). It wraps 7z.dll or any compatible one and makes use of LZMA SDK."_


## Issues 

In [nuget](https://www.nuget.org/packages/Squid-Box.SevenZipSharp#supportedframeworks-body-tab) this library is labeled as fully compliant with .Net Core 3.1, .Net Standard 2.0 and .Net Framework 4.5.
The compliance with .Net 6.0 and its associated toolset (e.g. PackageReference) is _derived_ from .Net Standard. 

In VS2022, adding this nuget dependency to the project, while compiles fine, it doesn't make it into the output - the SevenZipSharp DLL is not copied to the output folder of the project.

## Temporary solution

Until the nuget specification of SevenZipSharp DLL is updated for VS2022 and direct .Net 6.0 compliance, we'll perform the following workaround:

* Download the nuget manually and place it in this folder
  * Verify its integrity using the SHA-512 signature
* Expand its contents - namely the __lib_ folder
* Add the DLL in `./lib/netstandard2.0/` folder as a local dependency (assembly) to the project in VS2022

