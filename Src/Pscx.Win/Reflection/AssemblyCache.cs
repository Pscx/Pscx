//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Interfaces and Classes needed to support querying of
//              the Global Assembly Cache
//
// Creation Date: Jan 31, 2007
// Modifed: UCOMIStream -> IStream (UCOMIStream deprecated in 2.0)
//---------------------------------------------------------------------

using Pscx.Win.Interop;
using Pscx.Win.Interop.Fusion;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

// Source: Microsoft KB Article KB317540
// with modifications by oisin.

/*
SUMMARY
The native code application programming interfaces (APIs) that allow you to interact with the Global Assembly Cache (GAC) are not documented 
in the .NET Framework Software Development Kit (SDK) documentation. 

MORE INFORMATION
CAUTION: Do not use these APIs in your application to perform assembly binds or to test for the presence of assemblies or other run time, 
development, or design-time operations. Only administrative tools and setup programs must use these APIs. If you use the GAC, this directly 
exposes your application to assembly binding fragility or may cause your application to work improperly on future versions of the .NET 
Framework.

The GAC stores assemblies that are shared across all applications on a computer. The actual storage location and structure of the GAC is 
not documented and is subject to change in future versions of the .NET Framework and the Microsoft Windows operating system.

The only supported method to access assemblies in the GAC is through the APIs that are documented in this article.

Most applications do not have to use these APIs because the assembly binding is performed automatically by the common language runtime. 
Only custom setup programs or management tools must use these APIs. Microsoft Windows Installer has native support for installing assemblies
 to the GAC.

For more information about assemblies and the GAC, see the .NET Framework SDK.

Use the GAC API in the following scenarios: 
When you install an assembly to the GAC.
When you remove an assembly from the GAC.
When you export an assembly from the GAC.
When you enumerate assemblies that are available in the GAC.
NOTE: CoInitialize(Ex) must be called before you use any of the functions and interfaces that are described in this specification. 
*/

namespace Pscx.Win.Reflection {
    [SupportedOSPlatform("windows")]
    public class AssemblyCache : IDisposable {
        private readonly IAssemblyCache _native;

        #region GUID Definition

        /// <summary>
        /// GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// The assembly is referenced by an application that has been installed by using Windows Installer. 
        /// The szIdentifier field is set to MSI, and szNonCannonicalData is set to Windows Installer. 
        /// This scheme must only be used by Windows Installer itself.
        /// </summary>
        public static Guid FUSION_REFCOUNT_UNINSTALL_SUBKEY_GUID {
            get {
                return new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
            }
        }

        /// <summary>
        /// GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// 
        /// </summary>
        public static Guid FUSION_REFCOUNT_FILEPATH_GUID {
            get {
                return new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
            }
        }

        /// <summary>
        /// GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// 
        /// </summary>
        public static Guid FUSION_REFCOUNT_OPAQUE_STRING_GUID {
            get {
                return new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
            }
        }

        /// <summary>
        /// GUID value for element guidScheme in the struct FUSION_INSTALL_REFERENCE
        /// 
        /// </summary>
        public static Guid FUSION_REFCOUNT_MSI_GUID {
            get {
                return new Guid("25df0fc1-7f97-4070-add7-4b13bbfd7cb8");
            }
        }

        #endregion

        /// <summary>
        /// Use this method as a start for the GAC API
        /// </summary>
        /// <returns>IAssemblyCache COM interface</returns>
        public AssemblyCache() {
            NativeMethods.CreateAssemblyCache(out _native, 0);
        }

        public void Dispose() {
            Marshal.ReleaseComObject(_native);
        }

        public string QueryAssemblyPath(string displayName) {
            AssemblyInfo info = AssemblyInfo.Create();

            if (NativeMethods.SUCCESS == _native.QueryAssemblyInfo(QueryAsmInfoFlag.Validate, displayName, ref info)) {
                return info.pszCurrentAssemblyPathBuf;
            }

            return null;
        }

        public static string GlobalCachePath {
            get { return GetCachePath(AssemblyCacheType.Gac); }
        }
        public static IEnumerable<AssemblyName> GetGlobalAssemblies() {
            return GetGlobalAssemblies(null);
        }
        public static IEnumerable<AssemblyName> GetGlobalAssemblies(string partialName) {
            return GetAssemblies(partialName, AssemblyCacheType.Gac);
        }

        public static string DownloadCachePath {
            get { return GetCachePath(AssemblyCacheType.Download); }
        }
        public static IEnumerable<AssemblyName> GetDownloadedAssemblies() {
            return GetDownloadedAssemblies(null);
        }
        public static IEnumerable<AssemblyName> GetDownloadedAssemblies(string partialName) {
            return GetAssemblies(partialName, AssemblyCacheType.Download);
        }

        public static string NGenCachePath {
            get { return GetCachePath(AssemblyCacheType.NGen); }
        }
        public static IEnumerable<AssemblyName> GetNGenAssemblies() {
            return GetNGenAssemblies(null);
        }
        public static IEnumerable<AssemblyName> GetNGenAssemblies(string partialName) {
            return GetAssemblies(partialName, AssemblyCacheType.NGen);
        }

        public static IEnumerable<AssemblyName> GetAssemblies(AssemblyCacheType cacheType) {
            return GetAssemblies(null, cacheType);
        }
        public static IEnumerable<AssemblyName> GetAssemblies(string partialName, AssemblyCacheType cacheType) {
            IAssemblyEnum enumerator;
            IAssemblyName assemblyName = null;

            if (partialName != null) {
                assemblyName = AssemblyCache.CreateAssemblyName(partialName);
            }

            NativeMethods.CreateAssemblyEnum(out enumerator, IntPtr.Zero, assemblyName, cacheType, IntPtr.Zero);

            return new AssemblyCacheSearcher(enumerator);
        }

        private static IAssemblyName CreateAssemblyName(string name) {
            IAssemblyName an;

            // oisin: replaced 0x2 with CREATE_ASM_NAME_OBJ_FLAGS enum for clarity
            NativeMethods.CreateAssemblyNameObject(out an, name,
                CreateAssemblyNameFlags.SetDefaultValues, IntPtr.Zero);

            return an;
        }

        private static string GetCachePath(AssemblyCacheType cache) {
            int bufferSize = NativeMethods.MAX_PATH;
            StringBuilder buffer = new StringBuilder(bufferSize);

            NativeMethods.GetCachePath(cache, buffer, ref bufferSize);

            return buffer.ToString();
        }
    }
}
