//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Interfaces and Classes needed to support querying of
//              the Global Assembly Cache
//
// Creation Date: Jan 31, 2007
//---------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace Pscx.Interop
{
    /// <summary>
    /// The IAssemblyCache interface is the top-level interface that provides access to the GAC.
    /// </summary>
    [ComImport, Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCache
    {
        /// <summary>
        /// The IAssemblyCache::UninstallAssembly method removes a reference to an assembly from the GAC. 
        /// If other applications hold no other references to the assembly, the files that make up the assembly are removed from the GAC. 
        /// </summary>
        /// <param name="dwFlags">No flags defined. Must be zero.</param>
        /// <param name="pszAssemblyName">The name of the assembly. A zero-ended Unicode string.</param>
        /// <param name="pRefData">A pointer to a FUSION_INSTALL_REFERENCE structure. Although this is not recommended, 
        ///		this parameter can be null. The assembly is installed without an application reference, or all existing application 
        ///		references are gone.</param>
        /// <param name="pulDisposition">Pointer to an integer that indicates the action that is performed by the function.</param>
        /// <returns>The return values are defined as follows: 
        ///		S_OK - The assembly has been uninstalled.
        ///		S_FALSE - The operation succeeded, but the assembly was not removed from the GAC. 
        ///		The reason is described in pulDisposition.</returns>
        ///	<remarks>
        ///	NOTE: If pulDisposition is not null, pulDisposition contains one of the following values:
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED - The assembly files have been removed from the GAC.
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE - An application is using the assembly. 
        ///			This value is returned on Microsoft Windows 95 and Microsoft Windows 98.
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED - The assembly does not exist in the GAC.
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING - Not used.
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES - The assembly has not been removed from the GAC because 
        ///			another application reference exists.
        ///		IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND - The reference that is specified in pRefData is not found 
        ///			in the GAC.
        ///	</remarks>
        [PreserveSig]
        int UninstallAssembly(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            [MarshalAs(UnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData,
            out uint pulDisposition);

        /// <summary>
        /// The IAssemblyCache::QueryAssemblyInfo method retrieves information about an assembly from the GAC. 
        /// </summary>
        /// <param name="dwFlags">One of QUERYASMINFO_FLAG_VALIDATE or QUERYASMINFO_FLAG_GETSIZE: 
        ///		*_VALIDATE - Performs validation of the files in the GAC against the assembly manifest, including hash verification 
        ///			and strong name signature verification.
        ///		*_GETSIZE - Returns the size of all files in the assembly (disk footprint). If this is not specified, the 
        ///			ASSEMBLY_INFO::uliAssemblySizeInKB field is not modified.</param>
        /// <param name="pszAssemblyName"></param>
        /// <param name="pAsmInfo"></param>
        /// <returns></returns>
        [PreserveSig]
        int QueryAssemblyInfo(
            QueryAsmInfoFlag dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            ref AssemblyInfo pAsmInfo);

        /// <summary>
        /// Undocumented
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="pvReserved"></param>
        /// <param name="ppAsmItem"></param>
        /// <param name="pszAssemblyName"></param>
        /// <returns></returns>
        [PreserveSig]
        int CreateAssemblyCacheItem(
            uint dwFlags,
            IntPtr pvReserved,
            out IAssemblyCacheItem ppAsmItem,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);

        /// <summary>
        /// Undocumented
        /// </summary>
        /// <param name="ppAsmScavenger"></param>
        /// <returns></returns>
        [PreserveSig]
        int CreateAssemblyScavenger(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppAsmScavenger);

        /// <summary>
        /// The IAssemblyCache::InstallAssembly method adds a new assembly to the GAC. The assembly must be persisted in the file 
        /// system and is copied to the GAC.
        /// </summary>
        /// <param name="dwFlags">At most, one of the bits of the IASSEMBLYCACHE_INSTALL_FLAG_* values can be specified: 
        ///		*_REFRESH - If the assembly is already installed in the GAC and the file version numbers of the assembly being 
        ///		installed are the same or later, the files are replaced.
        ///		*_FORCE_REFRESH - The files of an existing assembly are overwritten regardless of their version number.</param>
        /// <param name="pszManifestFilePath"> A string pointing to the dynamic-linked library (DLL) that contains the assembly manifest. 
        ///	Other assembly files must reside in the same directory as the DLL that contains the assembly manifest.</param>
        /// <param name="pRefData">A pointer to a FUSION_INSTALL_REFERENCE that indicates the application on whose behalf the 
        /// assembly is being installed. Although this is not recommended, this parameter can be null, but this leaves the assembly 
        /// without any application reference.</param>
        /// <returns></returns>
        [PreserveSig]
        int InstallAssembly(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath,
            [MarshalAs(UnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData);
    }

}
