using System.Runtime.InteropServices;

namespace Pscx.Win.Interop.DirectoryServices
{
    [ComImport]
    [Guid("9068270b-0939-11d1-8be1-00c04fd8d503")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IADsLargeInteger
    {
        uint HighPart { get; set; }
        uint LowPart { get; set; }
    }
}
