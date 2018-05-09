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
    /// The IInstallReferenceEnum interface enumerates all references that are set on an assembly in the GAC.
    /// NOTE: References that belong to the assembly are locked for changes while those references are being enumerated. 
    /// </summary>
    [ComImport, Guid("56b1a988-7c0c-4aa2-8639-c3eb5a90226f"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInstallReferenceEnum
    {
        /// <summary>
        /// IInstallReferenceEnum::GetNextInstallReferenceItem returns the next reference information for an assembly. 
        /// </summary>
        /// <param name="ppRefItem">Pointer to a memory location that receives the IInstallReferenceItem pointer.</param>
        /// <param name="dwFlags">Must be zero.</param>
        /// <param name="pvReserved">Must be null.</param>
        /// <returns></returns>
        [PreserveSig()]
        int GetNextInstallReferenceItem(
            out IInstallReferenceItem ppRefItem,
            uint dwFlags,
            IntPtr pvReserved);
    }
}
