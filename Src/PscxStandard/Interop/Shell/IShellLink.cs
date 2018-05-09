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
    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CShellLink
    {
    }

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellLink
    {
        [Description("Retrieves the path and filename of a shell link object")]
        void GetPath(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            /*ref WIN32_FIND_DATA*/ IntPtr pfd,
            uint fFlags);

        [Description("Retrieves the list of shell link item identifiers")]
        void GetIDList(out IntPtr ppidl);

        [Description("Sets the list of shell link item identifiers")]
        void SetIDList(IntPtr pidl);

        [Description("Retrieves the shell link description string")]
        void GetDescription(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, 
            int cchMaxName);

        [Description("Sets the shell link description string")]
        void SetDescription(
           [MarshalAs(UnmanagedType.LPWStr)] string pszName);

        [Description("Retrieves the name of the shell link working directory")]
        void GetWorkingDirectory(
           [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
           int cchMaxPath);

        [Description("Sets the name of the shell link working directory")]
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        [Description("Retrieves the shell link command-line arguments")]
        void GetArguments(
           [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
           int cchMaxPath);

        [Description("Sets the shell link command-line arguments")]
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        [Description("Retrieves or sets the shell link hot key")]
        void GetHotkey(out short pwHotkey);
        
        [Description("Retrieves or sets the shell link hot key")]
        void SetHotkey(short pwHotkey);

        [Description("Retrieves or sets the shell link show command")]
        void GetShowCmd(out uint piShowCmd);
        
        [Description("Retrieves or sets the shell  link show command")]
        void SetShowCmd(uint piShowCmd);

        [Description("Retrieves the location (path and index) of the shell link icon")]
        void GetIconLocation(
           [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
           int cchIconPath,
           out int piIcon);

        [Description("Sets the location (path and index) of the shell link icon")]
        void SetIconLocation(
           [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
           int iIcon);

        [Description("Sets the shell link relative path")]
        void SetRelativePath(
           [MarshalAs(UnmanagedType.LPWStr)] 
            string pszPathRel,
           uint dwReserved);

        [Description("Resolves a shell link. The system " +
        "searches for the shell link object and updates " +
        "the shell link path and its list of " +
        "identifiers (if necessary)")]
        void Resolve(IntPtr hWnd,uint fFlags);

        [Description("Sets the shell link path and filename")]
        void SetPath([MarshalAs(UnmanagedType.LPWStr)]string pszFile);
    }
}
