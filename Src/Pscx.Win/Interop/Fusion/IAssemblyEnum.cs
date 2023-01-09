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

namespace Pscx.Win.Interop.Fusion
{
    /// <summary>
    /// The IAssemblyEnum interface enumerates the assemblies in the GAC.
    /// </summary>
    [ComImport, Guid("21b8916c-f28e-11d2-a473-00c04f8ef448"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyEnum
    {
        /// <summary>
        /// The IAssemblyEnum::GetNextAssembly method enumerates the assemblies in the GAC. 
        /// </summary>
        /// <param name="pvReserved">Must be null.</param>
        /// <param name="ppName">Pointer to a memory location that is to receive the interface pointer to the assembly 
        /// name of the next assembly that is enumerated.</param>
        /// <param name="dwFlags">Must be zero.</param>
        /// <returns></returns>
        [PreserveSig()]
        int GetNextAssembly(
            IntPtr pvReserved,
            out IAssemblyName ppName,
            uint dwFlags);

        /// <summary>
        /// Undocumented. Best guess: reset the enumeration to the first assembly.
        /// </summary>
        /// <returns></returns>
        [PreserveSig()]
        int Reset();

        /// <summary>
        /// Undocumented. Create a copy of the assembly enum that is independently enumerable.
        /// </summary>
        /// <param name="ppEnum"></param>
        /// <returns></returns>
        [PreserveSig()]
        int Clone(
            out IAssemblyEnum ppEnum);
    }
}
