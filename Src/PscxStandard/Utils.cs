//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement some generic error utilities.
//
// Creation Date: Sept 12, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Pscx.Interop;
using System.Globalization;
using System.ComponentModel;

namespace Pscx
{
    public static class Utils
    {
        private const int MaxPath = 260;

        public static FileSystemInfo GetFileOrDirectory(String path)
        {
            var fi = new FileInfo(path);

            if (fi.Exists)
            {
                return fi;
            }

            var di = new DirectoryInfo(path);

            if (di.Exists)
            {
                return di;
            }

            return null;
        }

        public static string GetShortPathName(FileSystemInfo info)
        {
            PscxArgumentException.ThrowIfIsNull(info);

            if (!info.Exists)
            {
                if (info is DirectoryInfo)
                {
                    throw new DirectoryNotFoundException();
                }

                throw new FileNotFoundException();
            }

            StringBuilder buffer = new StringBuilder(MaxPath);
            bool result = NativeMethods.GetShortPathName(info.FullName, buffer, (uint)(buffer.Capacity));
            
            if (!result)
            {
                throw new Win32Exception();
            }

            return buffer.ToString();
        }

        public static string GetShortPathName(string path)
        {
            PscxArgumentException.ThrowIfIsNull(path);
            FileSystemInfo info = GetFileOrDirectory(path);

            if (info == null)
            {
                throw new FileNotFoundException();
            }

            return GetShortPathName(info);
        }

        public static void AdjustTokenPrivileges(SafeTokenHandle hToken, TokenPrivilegeCollection privileges)
        {
            byte[] buffer = privileges.ToTOKEN_PRIVILEGES();
            if (!NativeMethods.AdjustTokenPrivileges(hToken,
                                                     false,
                                                     buffer,
                                                     buffer.Length,
                                                     IntPtr.Zero,
                                                     IntPtr.Zero))
            {
                throw PscxException.LastWin32Exception();
            }
        }

        public static T GetAttribute<T>(ICustomAttributeProvider provider)
            where T : Attribute
        {
            return GetAttribute<T>(provider, false);
        }

        public static T GetAttribute<T>(ICustomAttributeProvider provider, bool inherit)
            where T : Attribute
        {
            T[] attrs = GetAttributes<T>(provider, inherit);
            
            if (attrs == null || attrs.Length == 0)
            {
                return null;
            }

            return attrs[0];
        }

        public static T[] GetAttributes<T>(ICustomAttributeProvider provider, bool inherit)
        {
            object[] attrs = provider.GetCustomAttributes(typeof(T), inherit);

            if (attrs.Length == 0)
            {
                return null;
            }
            return attrs as T[];
        }

        public static T PtrToStructure<T>(IntPtr ptr) where T : struct
        {
            return (T)(Marshal.PtrToStructure(ptr, typeof(T)));
        }

        public static IntPtr IncrementPointer<T>(IntPtr ptr) where T : struct
        {
            return new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(T)));
        }

        public static IEnumerable<T> ReadNativeArray<T>(IntPtr ptr, int length) where T : struct
        {
            IntPtr current = ptr;

            for (int i = 0; i < length; i++)
            {
                yield return PtrToStructure<T>(current);
                current = IncrementPointer<T>(current);
            }
        }

        public static long MakeLong(uint high, uint low)
        {
            return (high << 32) | (low);
        }

        public static int HighWord(int value)
        {
            return value >> 16;
        }

        public static int LowWord(int value)
        {
            return value & 0xffff;
        }

        public static TEnum ParseEnumOrThrow<TEnum>(string value)
        {
            PscxArgumentException.ThrowIfIsNotDefined(typeof(TEnum), value);

            return (TEnum)(Enum.Parse(typeof(TEnum), value));
        }

        public static string GetEnumName(Enum value)
        {
            return Enum.GetName(value.GetType(), value);
        }

        public static void DoubleCheckInit<T>(ref T obj, object syncRoot, Producer<T> init)
        {
            if (Equals(obj, default(T)))
            {
                Thread.MemoryBarrier();

                lock (syncRoot)
                {
                    if (Equals(obj, default(T)))
                    {
                        obj = init();
                    }
                }
            }
        }

        public static string FormatInvariant(string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static void SplitString(string str, int middlePoint, out string left, out string right)
        {
            left = right = string.Empty;
            
            if (!string.IsNullOrEmpty(str))
            {
                if (middlePoint <= 0)
                {
                    right = str;
                }
                else if (middlePoint >= str.Length - 1)
                {
                    left = str;
                }
                else
                {
                    left = str.Substring(0, middlePoint);
                    right = str.Substring(middlePoint + 1);
                }
            }
        }

        public static object UnwrapPSObject(object wrappedObject)
        {
            return UnwrapPSObject<object>(wrappedObject);
        }

        public static T UnwrapPSObject<T>(object wrappedObject)
        {
            if (wrappedObject != null)
            {
                if (wrappedObject is PSObject)
                {
                    // ensure incoming object is fully unwrapped
                    // bugs in powershell mean that incoming objects may
                    // or may not have a PSObject wrapper.
                    object immediateBaseObject;
                    var temp = (PSObject) wrappedObject;
                    do
                    {
                        immediateBaseObject = temp.ImmediateBaseObject;
                        temp = immediateBaseObject as PSObject;
                    } while (temp != null);

                    wrappedObject = immediateBaseObject;
                }
            }
            return (T)wrappedObject;
        }
    }
}