using System;
using System.Runtime.InteropServices;

namespace Pscx.Interop
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PropVariant
    {
        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);

        [FieldOffset(0)]
        public ushort vt;
        [FieldOffset(8)]
        public IntPtr pointerValue;
        [FieldOffset(8)]
        public byte byteValue;
        [FieldOffset(8)]
        public long longValue;
        [FieldOffset(8)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;

        public VarEnum VarType
        {
            get { return (VarEnum)vt; }
        }

        public void Clear()
        {
            switch (VarType)
            {
                case VarEnum.VT_EMPTY:
                    break;
                case VarEnum.VT_NULL:
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                case VarEnum.VT_R4:
                case VarEnum.VT_R8:
                case VarEnum.VT_CY:
                case VarEnum.VT_DATE:
                case VarEnum.VT_ERROR:
                case VarEnum.VT_BOOL:
                    //case VarEnum.VT_DECIMAL:
                case VarEnum.VT_I1:
                case VarEnum.VT_UI1:
                case VarEnum.VT_UI2:
                case VarEnum.VT_UI4:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI8:
                case VarEnum.VT_INT:
                case VarEnum.VT_UINT:
                case VarEnum.VT_HRESULT:
                case VarEnum.VT_FILETIME:
                    vt = 0;
                    break;
                default:
                    PropVariantClear(ref this);
                    break;
            }
        }

        public object GetObject()
        {
            switch (VarType)
            {
                case VarEnum.VT_EMPTY:
                    return null;
                case VarEnum.VT_FILETIME:
                    return DateTime.FromFileTime(longValue);
                default:
                    GCHandle PropHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
                    try
                    {
                        return Marshal.GetObjectForNativeVariant(PropHandle.AddrOfPinnedObject());
                    }
                    finally
                    {
                        PropHandle.Free();
                    }
            }
        }
    }
}