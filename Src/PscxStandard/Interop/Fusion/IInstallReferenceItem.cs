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
    /// The IInstallReferenceItem interface represents a reference that has been set on an assembly in the GAC. 
    /// Instances of IInstallReferenceIteam are returned by the IInstallReferenceEnum interface.
    /// </summary>
    [ComImport, Guid("582dac66-e678-449f-aba6-6faaec8a9394"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInstallReferenceItem
    {
        /// <summary>
        /// The IInstallReferenceItem::GetReference method returns a FUSION_INSTALL_REFERENCE structure. 
        /// </summary>
        /// <param name="ppRefData">A pointer to a FUSION_INSTALL_REFERENCE structure. The memory is allocated by the GetReference 
        /// method and is freed when IInstallReferenceItem is released. Callers must not hold a reference to this buffer after the 
        /// IInstallReferenceItem object is released.</param>
        /// <param name="dwFlags">Must be zero.</param>
        /// <param name="pvReserved">Must be null.</param>
        /// <returns></returns>
        [PreserveSig]
        int GetReference(
            [MarshalAs(UnmanagedType.LPArray)] out FUSION_INSTALL_REFERENCE[] ppRefData,
            uint dwFlags,
            IntPtr pvReserved);
    }
}
