using System;
using System.Runtime.InteropServices;
using System.Text;
using Pscx.Commands;

namespace Pscx
{
    internal static class EncodingConversion
    {
        internal static Encoding Convert(PscxCmdlet cmdlet, string encoding, string parameterName)
        {
            if (string.IsNullOrEmpty(encoding) || string.Equals(encoding, "unknown", StringComparison.OrdinalIgnoreCase) || (string.Equals(encoding, "string", StringComparison.OrdinalIgnoreCase) || string.Equals(encoding, "unicode", StringComparison.OrdinalIgnoreCase)))
                return Encoding.Unicode;
            if (string.Equals(encoding, "bigendianunicode", StringComparison.OrdinalIgnoreCase))
                return Encoding.BigEndianUnicode;
            if (string.Equals(encoding, "utf8", StringComparison.OrdinalIgnoreCase))
                return Encoding.UTF8;
            if (string.Equals(encoding, "ascii", StringComparison.OrdinalIgnoreCase))
                return Encoding.ASCII;
            if (string.Equals(encoding, "utf7", StringComparison.OrdinalIgnoreCase))
                return Encoding.UTF7;
            if (string.Equals(encoding, "utf32", StringComparison.OrdinalIgnoreCase))
                return Encoding.UTF32;
            if (string.Equals(encoding, "default", StringComparison.OrdinalIgnoreCase))
                return Encoding.Default;
            if (string.Equals(encoding, "oem", StringComparison.OrdinalIgnoreCase))
                return Encoding.GetEncoding((int)EncodingConversion.NativeMethods.GetOEMCP());
            string str = string.Join(", ", "unknown", "string", "unicode", "bigendianunicode", "ascii", "utf8", "utf7", "utf32", "default", "oem");
            cmdlet.ErrorHandler.ThrowInvalidFileEncodingArgument(parameterName, encoding, str);
            return (Encoding)null;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern uint GetOEMCP();
        }
    }
}
