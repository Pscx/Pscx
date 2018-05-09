using System;
using System.Runtime.InteropServices;

namespace Pscx.Interop.SevenZip
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int CreateObjectDelegate([In] ref Guid classID, [In] ref Guid interfaceID, out IntPtr outObject);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int GetHandlerProperty2Delegate(uint formatIndex, ArchivePropId propID, ref PropVariant value);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int GetHandlerPropertyDelegate(ArchivePropId propID, ref PropVariant value);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int GetNumberOfFormatsDelegate(out uint numFormats);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int CreateObjectAsIntPtrDelegate([In] ref Guid classID, [In] ref Guid interfaceID, out IntPtr outObject);

    // GetStream OUT: S_OK - OK, S_FALSE - skip this file
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000600200000")]
    public interface IArchiveExtractCallback
    {
        void SetTotal(ulong total);
        void SetCompleted([In] ref ulong completeValue);
        [PreserveSig]
        int GetStream(uint index, [MarshalAs(UnmanagedType.Interface)] out ISequentialOutStream outStream, AskMode askExtractMode);
        void PrepareOperation(AskMode askExtractMode);
        void SetOperationResult(OperationResult resultEOperationResult);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600100000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IArchiveOpenCallback
    {
        void SetTotal(IntPtr files, IntPtr bytes);
        void SetCompleted(IntPtr files, IntPtr bytes);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600300000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IArchiveOpenVolumeCallback
    {
        void GetProperty(ItemPropId propID, IntPtr value);
        [PreserveSig]
        int GetStream([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Interface)] out IInStream inStream);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600800000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IArchiveUpdateCallback
    {
        void SetTotal(ulong total);
        void SetCompleted([In] ref ulong completeValue);
        void GetUpdateItemInfo(int index, out int newData, out int newProperties, out uint indexInArchive);
        void GetProperty(int index, ItemPropId propID, IntPtr value);
        void GetStream(int index, out ISequentialInStream inStream);
        void SetOperationResult(int operationResult);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600820000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IArchiveUpdateCallback2
    {
        void SetTotal(ulong total);
        void SetCompleted([In] ref ulong completeValue);
        void GetUpdateItemInfo(int index, out int newData, out int newProperties, out uint indexInArchive);
        void GetProperty(int index, ItemPropId propID, IntPtr value);
        void GetStream(int index, out ISequentialInStream inStream);
        void SetOperationResult(int operationResult);
        void GetVolumeSize(int index, out ulong size);
        void GetVolumeStream(int index, out ISequentialOutStream volumeStream);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000500100000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICryptoGetTextPassword
    {
        [PreserveSig]
        int CryptoGetTextPassword([MarshalAs(UnmanagedType.BStr)] out string password);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000500110000")]
    public interface ICryptoGetTextPassword2
    {
        void CryptoGetTextPassword2([MarshalAs(UnmanagedType.Bool)] out bool passwordIsDefined, [MarshalAs(UnmanagedType.BStr)] out string password);
    }

    // Extract(...)
    // indices must be sorted 
    // numItems = 0xFFFFFFFF means all files
    // testMode != 0 means "test files operation"
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000600600000")]
    public interface IInArchive
    {
        [PreserveSig]
        int Open(IInStream stream, [In] ref ulong maxCheckStartPosition, [MarshalAs(UnmanagedType.Interface)] IArchiveOpenCallback openArchiveCallback);
        [PreserveSig]
        int Close();
        uint GetNumberOfItems();
        void GetProperty(uint index, ItemPropId propID, ref PropVariant value);
        [PreserveSig]
        int Extract([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] indices, uint numItems, int testMode, [MarshalAs(UnmanagedType.Interface)] IArchiveExtractCallback extractCallback);
        void GetArchiveProperty(uint propID, ref PropVariant value);
        uint GetNumberOfProperties();
        void GetPropertyInfo(uint index, [MarshalAs(UnmanagedType.BStr)] out string name, out ItemPropId propID, out ushort varType);
        uint GetNumberOfArchiveProperties();
        void GetArchivePropertyInfo(uint index, [MarshalAs(UnmanagedType.BStr)] string name, ref uint propID, ref ushort varType);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600400000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInArchiveGetStream
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        ISequentialInStream GetStream(uint index);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000300030000")]
    public interface IInStream
    {
        uint Read(IntPtr data, uint size);
        void Seek(long offset, uint seekOrigin, IntPtr newPosition);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000600A00000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOutArchive
    {
        void UpdateItems(ISequentialOutStream outStream, int numItems, IArchiveUpdateCallback updateCallback);
        FileTimeType GetFileTimeType();
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000300040000")]
    public interface IOutStream
    {
        [PreserveSig]
        int Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size, IntPtr processedSize);
        void Seek(long offset, uint seekOrigin, IntPtr newPosition);
        [PreserveSig]
        int SetSize(long newSize);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000300070000")]
    public interface IOutStreamFlush
    {
        void Flush();
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000000050000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProgress
    {
        void SetTotal(ulong total);
        void SetCompleted([In] ref ulong completeValue);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000300010000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISequentialInStream
    {
        uint Read(IntPtr data, uint size);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000300020000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISequentialOutStream
    {
        [PreserveSig]
        int Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size, IntPtr processedSize);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("23170F69-40C1-278A-0000-000600030000")]
    public interface ISetProperties
    {
        void SetProperties([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] names, IntPtr values, int numProperties);
    }

    [ComImport, Guid("23170F69-40C1-278A-0000-000300060000"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamGetSize
    {
        ulong GetSize();
    }
}