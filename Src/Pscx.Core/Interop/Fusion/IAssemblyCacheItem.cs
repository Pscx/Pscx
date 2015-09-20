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
using System.Runtime.InteropServices.ComTypes;

namespace Pscx.Interop
{
    /// <summary>
    /// Undocumented. Probably only for internal use.
    /// <see cref="IAssemblyCache.CreateAssemblyCacheItem"/>
    /// </summary>
    [ComImport, Guid("9E3AAEB4-D1CD-11D2-BAB9-00C04F8ECEAE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCacheItem
    {
        /// <summary>
        /// Undocumented.
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="pszStreamName"></param>
        /// <param name="dwFormat"></param>
        /// <param name="dwFormatFlags"></param>
        /// <param name="ppIStream"></param>
        /// <param name="puliMaxSize"></param>
        void CreateStream(
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszStreamName,
            uint dwFormat,
            uint dwFormatFlags,
            out /*UCOMIStream*/ IStream ppIStream,
            ref long puliMaxSize);

        /// <summary>
        /// Undocumented.
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="pulDisposition"></param>
        void Commit(
            uint dwFlags,
            out long pulDisposition);

        /// <summary>
        /// Undocumented.
        /// </summary>
        void AbortItem();
    }
}
