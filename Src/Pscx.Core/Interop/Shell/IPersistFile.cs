//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Interfaces needed to support shell shortcuts
//
// Creation Date: Dec 13, 2006
//
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Pscx.Interop
{
    [ComImport]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistFile
    {
        // can't get this to go if I extend IPersist, so put it here:
        [PreserveSig]
        int GetClassID(out Guid pClassID);

        [Description("Checks for changes since last file write")]
        [PreserveSig]
        int IsDirty();

        [Description("Opens the specified file and initializes the object from its contents")]
        void Load(
           [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
           uint dwMode);

        [Description("Saves the object into the specified file")]
        void Save(
           [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
           [MarshalAs(UnmanagedType.Bool)]   bool fRemember);

        [Description("Notifies the object that save is completed")]
        void SaveCompleted(
           [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        [Description("Gets the current name of the file associated with the object")]
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}
