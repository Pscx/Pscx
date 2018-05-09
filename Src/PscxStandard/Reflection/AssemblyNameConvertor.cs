//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Converts native IAssemblyNames to managed AssemblyNames.
//
// Creation Date: Jan 31, 2007
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Pscx.Interop;

namespace Pscx.Reflection
{
    static class AssemblyNameConvertor
    {
        public static AssemblyName ToAssemblyName(IAssemblyName native)
        {
            string displayName = GetDisplayName(native);

            using (AssemblyCache cache = new AssemblyCache())
            {
                // according to MSKB, this is a SUPPORTED method, therefore we can use the obtained path
                // in the ASSEMBLY_INFO struct to generate a fully-populated AssemblyName object.
                // see: http://support.microsoft.com/kb/317540 

                string path = cache.QueryAssemblyPath(displayName);

                if (path != null)
                {
                    return AssemblyName.GetAssemblyName(path);
                }
            }

            return new AssemblyName(displayName);
        }

        private static string GetDisplayName(IAssemblyName native)
        {
            int bufferSize = 1024;
            StringBuilder buffer = new StringBuilder(bufferSize);

            AssemblyDisplayFlags dwDisplayFlags = AssemblyDisplayFlags.ProcessorArchitecture |
                                                  AssemblyDisplayFlags.PublicKeyToken |
                                                  AssemblyDisplayFlags.LanguageId |
                                                  AssemblyDisplayFlags.Culture |
                                                  AssemblyDisplayFlags.Version;

            if (NativeMethods.SUCCESS == native.GetDisplayName(buffer, ref bufferSize, dwDisplayFlags))
            {
                return buffer.ToString();
            }

            return null;
        }
    }
}
