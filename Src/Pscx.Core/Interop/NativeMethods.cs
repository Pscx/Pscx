//---------------------------------------------------------------------
// Original Author: TonyDeSweet
//
// Description: Helper class for PInvoke Win32API.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------
using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

using Microsoft.Win32.SafeHandles;

using ComTypes = System.Runtime.InteropServices.ComTypes;

// I don't care if there are some values that aren't used yet in this interop class.
#pragma warning disable 0414

namespace Pscx.Interop
{
    static class Dll
    {
        public const string Advapi32 = "advapi32.dll";
        public const string Fusion = "Fusion.dll";
        public const string Kernel32 = "kernel32.dll";
        public const string NetApi32 = "NetApi32.dll";
        public const string NtdsApi = "NtdsApi.dll";
        public const string User32 = "user32.dll";
        public const string WtsApi32 = "WtsApi32.dll";
        public const string Ole32 = "ole32.dll";
    }

    public static partial class NativeMethods
    {
        public static void EnforceSuccess(int retval)
        {
            if (retval != SUCCESS)
            {
                throw new System.ComponentModel.Win32Exception(retval);
            }
        }

        #region Constants
        public const int FALSE = 0;
        public const int TRUE = 1;

        public const int SUCCESS = 0x0;
        public const int ERROR_INVALID_PARAMETER = 0x57;
        public const int ERROR_ACCESS_DENIED = 0x5;
        public const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
        public const int ERROR_BAD_LENGTH = 0x18;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        public const int ERROR_NONE_MAPPED = 0x534;
        public const int ERROR_NO_TOKEN = 0x3f0;
        public const int ERROR_NOT_ALL_ASSIGNED = 0x514;
        public const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
        public const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;

        public const int SYMLINK_FLAG_DIRECTORY = 1;

        public const int MAX_PATH = 260;
        #endregion

        #region advapi32

        [DllImport(Dll.Advapi32, EntryPoint = "LookupPrivilegeValueW", CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            ref LUID Luid);

        [DllImport(Dll.Advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool LookupPrivilegeName(
            string lpSystemName,
            ref LUID lpLuid,
            StringBuilder lpName,
            ref int cbName);

        [DllImport(Dll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool OpenProcessToken(
            IntPtr hProcess,
            TokenAccessLevels DesiredAccess,
            ref SafeTokenHandle TokenHandle);

        [DllImport(Dll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool OpenThreadToken(
            IntPtr ThreadToken,
            TokenAccessLevels DesiredAccess,
            bool OpenAsSelf,
            ref SafeTokenHandle TokenHandle);

        [DllImport(Dll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool LogonUser(
          string principal,
          string authority,
          string password,
          LogonTypes logonType,
          LogonProviders logonProvider,
          ref SafeTokenHandle token);

        #endregion

        #region advapi32!AdjustTokenPrivileges
        [DllImport(Dll.Advapi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool AdjustTokenPrivileges(
            SafeTokenHandle TokenHandle,
            bool DisableAllPrivileges,
            byte[] NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        #endregion

        #region user32

        [DllImport(Dll.User32)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern IntPtr GetOpenClipboardWindow();

        [DllImport(Dll.User32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        #endregion

        #region User32!GetClientRect
        [DllImport(Dll.User32)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        #endregion

        #region User32!GetWindowRect
        [DllImport(Dll.User32)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        #endregion

        #region User32!ClientToScreen
        [DllImport(Dll.User32)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
        #endregion

        #region User32!SetWindowPos
        [DllImport(Dll.User32)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            SetWindowPosFlags uFlags
        );
        #endregion

        // kernel32

        #region kernel32

        [DllImport(Dll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeLibraryHandle LoadLibrary(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport(Dll.Kernel32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(
            SafeLibraryHandle hModule,
            [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport(Dll.Kernel32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(Dll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetShortPathName(
           [MarshalAs(UnmanagedType.LPTStr)] String lpszLongPath,
           [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
           uint cchBuffer);


        [DllImport(Dll.Kernel32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CreateHardLink(
            string lpszHardLinkPath,
            string lpszExistingFileName,
            IntPtr lpSecurityAttributes);

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool DeleteFile(string lpFileName);

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool RemoveDirectory(string lpPathName);

        [DllImport(Dll.Kernel32)]
        public static extern Size GetConsoleFontSize(SafeFileHandle hConsoleOutput, int nFont);

        [DllImport(Dll.Kernel32)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport(Dll.Kernel32)]
        public static extern bool GetCurrentConsoleFont(SafeFileHandle hConsoleOutput, bool bMaximumWindow, out ConsoleFontInfo lpConsoleCurrentFont);

        #endregion

        #region kernel32!CreateFile

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [Flags]
        public enum FileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            GenericReadWrite = GenericRead | GenericWrite,
        }

        [Flags]
        public enum FileShare : uint
        {
            None = 0,
            Read = 1,
            Write = 2,
            Delete = 4,

            ReadWrite = Read | Write,
        }

        public enum CreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }

        [Flags]
        public enum FileAttributes : uint
        {
            None = 0,
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }
        #endregion

        #region kernel32!CreateConsoleScreenBuffer

        public enum ConsoleScreenBufferFlags
        {
            TextModeBuffer = 1,
        }

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern SafeFileHandle CreateConsoleScreenBuffer(
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ConsoleScreenBufferFlags dwFlags,
            IntPtr lpScreenBufferData
        );

        #endregion

        #region kernel32!SetConsoleActiveScreenBuffer

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool SetConsoleActiveScreenBuffer(SafeFileHandle hConsoleOutput);

        #endregion

        #region kernel32!GetConsoleMode

        [DllImport(Dll.Kernel32, EntryPoint = "GetConsoleMode", SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool GetConsoleInputMode(SafeFileHandle handle, out ConsoleInputModeFlags flags);

        [DllImport(Dll.Kernel32, EntryPoint = "GetConsoleMode", SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool GetConsoleOutputMode(SafeFileHandle handle, out ConsoleOutputModeFlags flags);

        [Flags]
        public enum ConsoleOutputModeFlags : uint
        {
            EnableProcessedOutput = 1,
            EnableWrapAtEolOutput = 2,
        }

        [Flags]
        public enum ConsoleInputModeFlags : uint
        {
            EnableProcessedInput = 0x0001,
            EnableLineInput = 0x0002,
            EnableEchoInput = 0x0004,
            EnableWindowInput = 0x0008,
            EnableMouseInput = 0x0010,
        }

        #endregion

        #region kernel32!ReadConsoleInput

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool ReadConsoleInput(
            SafeFileHandle hConsoleInput,
            InputRecord[] lpBuffer,
            int nLength,
            ref int lpNumberOfEventsRead
        );

        #endregion

        #region kernel32!GetStdHandle

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern SafeFileHandle GetStdHandle(StdHandle nStdHandle);

        public enum StdHandle : int
        {
            StdInput = -10,
            StdOutput = -11,
            StdError = -12,
        }

        #endregion

        #region kernel32!FindFirstVolumeMountPoint
        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFindVolumeMountPointHandle FindFirstVolumeMountPoint(
            string lpszRootPathName,
            StringBuilder lpszVolumeMountPointName,
            int cchStringBufferLength);
        #endregion

        #region kernel32!FindNextVolumeMountPoint
        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool FindNextVolumeMountPoint(
            SafeFindVolumeMountPointHandle hFindVolume,
            StringBuilder lpszVolumeMountPointName,
            int cchStringBufferLength);
        #endregion

        #region kernel32!FindVolumeMountPointClose
        [DllImport(Dll.Kernel32, SetLastError = true)]
        public static extern bool FindVolumeMountPointClose(IntPtr hFindVolumeMountPoint);
        #endregion

        #region kernel32!GetVolumeNameForVolumeMountPoint
        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetVolumeNameForVolumeMountPoint(
            string lpszVolumeMountPoint,
            StringBuilder lpszVolumeName,
            int cchStringBufferLength);

        #endregion

        #region kernel32!GetCurrentProcess
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();
        #endregion

        #region kernel32!GetModuleHandle
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region kernel32!GetProcAddress
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);
        #endregion

        #region kernel32!IsWow64Process
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
             [In] IntPtr hProcess,
             [Out] out bool wow64Process);
        #endregion

        #region kernel32!DeleteVolumeMountPoint
        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool DeleteVolumeMountPoint(string lpwszVolumeMountPoint);
        #endregion

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverLapped);
        [return: MarshalAs(UnmanagedType.Bool)]

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverLapped);

        // WtsApi32

        #region WtsApi32!WTSOpenServer
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr WTSOpenServer(string ServerName);
        #endregion

        #region WtsApi32!WTSCloseServer
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void WTSCloseServer(IntPtr hServer);
        #endregion

        #region WtsApi32!WTSGetActiveConsoleSessionId
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int WTSGetActiveConsoleSessionId();
        #endregion

        #region WtsApi32!WTSDisconnectSession
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSDisconnectSession(
            IntPtr hServer,
            int SessionId,
            bool bWait);
        #endregion

        #region WtsApi32!WTSLogoffSession
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSLogoffSession(
            IntPtr hServer,
            int SessionId,
            bool bWait);
        #endregion

        #region WtsApi32!WTSShutdownSystem
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSShutdownSystem(
            IntPtr hServer,
            int type);
        #endregion

        #region WtsApi32!WTSTerminateProcess
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSTerminateProcess(
            IntPtr hServer,
            int ProcessId,
            int ExitCode);
        #endregion

        #region WtsApi32!WTSEnumerateSessions
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSEnumerateSessions(
            IntPtr hServer,
            int Reserved,
            int Version,
            out IntPtr ppSessionInfo,
            out int pCount);
        #endregion

        #region WtsApi32!WTSFreeMemory
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void WTSFreeMemory(IntPtr pMemory);
        #endregion

        #region WtsApi32!WTSQuerySessionInformation
        [DllImport(Dll.WtsApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WTSQuerySessionInformation(
            IntPtr hServer,
            Int32 SessionId,
            WtsSessionInfoClass info,
            out IntPtr ppBuffer,
            out Int32 pBytesReturned);
        #endregion

        // Fusion

        #region Fusion

        /// <summary>
        /// The key entry point for reading the assembly cache.
        /// </summary>
        /// <param name="ppAsmCache">Pointer to return IAssemblyCache</param>
        /// <param name="dwReserved">must be 0</param>
        [DllImport(Dll.Fusion, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);

        /// <summary>
        /// An instance of IAssemblyName is obtained by calling the CreateAssemblyNameObject API.
        /// </summary>
        /// <param name="ppAssemblyNameObj">Pointer to a memory location that receives the IAssemblyName pointer that is created.</param>
        /// <param name="szAssemblyName">A string representation of the assembly name or of a full assembly reference that is 
        /// determined by dwFlags. The string representation can be null.</param>
        /// <param name="dwFlags">Zero or more of the bits that are defined in the CREATE_ASM_NAME_OBJ_FLAGS enumeration.</param>
        /// <param name="pvReserved"> Must be null.</param>
        [DllImport(Dll.Fusion, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void CreateAssemblyNameObject(out IAssemblyName ppAssemblyNameObj, string szAssemblyName,
            CreateAssemblyNameFlags dwFlags, IntPtr pvReserved);

        /// <summary>
        /// To obtain an instance of the CreateAssemblyEnum API, call the CreateAssemblyNameObject API.
        /// </summary>
        /// <param name="pEnum">Pointer to a memory location that contains the IAssemblyEnum pointer.</param>
        /// <param name="pUnkReserved">Must be null.</param>
        /// <param name="pName">An assembly name that is used to filter the enumeration. Can be null to enumerate all assemblies in the GAC.</param>
        /// <param name="dwFlags">Exactly one bit from the ASM_CACHE_FLAGS enumeration.</param>
        /// <param name="pvReserved">Must be NULL.</param>
        [DllImport(Dll.Fusion, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void CreateAssemblyEnum(out IAssemblyEnum pEnum, IntPtr pUnkReserved, IAssemblyName pName,
            AssemblyCacheType dwFlags, IntPtr pvReserved);

        /// <summary>
        /// To obtain an instance of the CreateInstallReferenceEnum API, call the CreateInstallReferenceEnum API.
        /// </summary>
        /// <param name="ppRefEnum">A pointer to a memory location that receives the IInstallReferenceEnum pointer.</param>
        /// <param name="pName">The assembly name for which the references are enumerated.</param>
        /// <param name="dwFlags"> Must be zero.</param>
        /// <param name="pvReserved">Must be null.</param>
        [DllImport(Dll.Fusion, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void CreateInstallReferenceEnum(out IInstallReferenceEnum ppRefEnum, IAssemblyName pName,
            uint dwFlags, IntPtr pvReserved);

        /// <summary>
        /// The GetCachePath API returns the storage location of the GAC. 
        /// </summary>
        /// <param name="dwCacheFlags">Exactly one of the bits defined in the ASM_CACHE_FLAGS enumeration.</param>
        /// <param name="pwzCachePath">Pointer to a buffer that is to receive the path of the GAC as a Unicode string.</param>
        /// <param name="pcchPath">Length of the pwszCachePath buffer, in Unicode characters.</param>
        [DllImport(Dll.Fusion, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetCachePath(AssemblyCacheType dwCacheFlags, StringBuilder pwzCachePath, ref int pcchPath);

        #endregion

        // NetApi32

        #region NetApi32!NetGetJoinInformation

        [DllImport(Dll.NetApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int NetGetJoinInformation(
          string pServer,
          out IntPtr ppNameBuffer,
          out ComputerJoinStatus pBufferType
        );

        #endregion

        #region NetApi32!NetApiBufferFree

        [DllImport(Dll.NetApi32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        #endregion

        // Ole32

        [DllImport(Dll.Ole32)]
        public static extern int GetRunningObjectTable(int reserved,
            out IRunningObjectTable prot);

        [DllImport(Dll.Ole32)]
        public static extern int CreateBindCtx(int reserved,
            out IBindCtx ppbc);

        [DllImport(Dll.Ole32, PreserveSig = false)]
        public static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

        [DllImport(Dll.Ole32, PreserveSig = false)]
        public static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

        [DllImport(Dll.Ole32)]
        public static extern int ProgIDFromCLSID([In()]ref Guid clsid, [MarshalAs(UnmanagedType.LPWStr)]out string lplpszProgID);

#if FALSE
        #region WtsApi32

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSEnumerateServers(
            IntPtr pDomainName,
            long Reserved,
            long Version,
            IntPtr ppServerInfo,
            IntPtr pCount);



        [DllImport(Dll.WtsApi32)]
        public static extern int WTSEnumerateProcesses(
            IntPtr hServer,
            long Reserved,
            long Version,
            IntPtr ppProcessInfo,
            IntPtr pCount);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSQueryUserConfig(
            IntPtr pServerName,
            IntPtr pUserName,
            UserConfiguration WTSConfigClass,
            IntPtr ppBuffer,
            IntPtr pBytesReturned);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSSetUserConfig(
            IntPtr pServerName,
            IntPtr pUserName,
            UserConfiguration WTSConfigClass,
            IntPtr pBuffer,
            long DataLength);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSSendMessage(
            IntPtr hServer,
            long SessionId,
            IntPtr pTitle,
            long TitleLength,
            IntPtr pMessage,
            long MessageLength,
            long Style,
            long Timeout,
            IntPtr pResponse,
            bool bWait);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSWaitSystemEvent(
            IntPtr hServer,
            long EventMask,
            IntPtr pEventFlags);

        [DllImport(Dll.WtsApi32)]
        public static extern IntPtr WTSVirtualChannelOpen(
            IntPtr hServer,
            long SessionId,
            IntPtr pVirtualName/* ascii name */);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelClose(IntPtr hChannelHandle);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelRead(
            IntPtr hChannelHandle,
            IntPtr TimeOut,
            IntPtr Buffer,
            IntPtr BufferSize,
            IntPtr pBytesRead);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelWrite(
            IntPtr hChannelHandle,
            IntPtr Buffer,
            IntPtr Length,
            IntPtr pBytesWritten);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelPurgeInput(IntPtr hChannelHandle);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelPurgeOutput(IntPtr hChannelHandle);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSVirtualChannelQuery(
            IntPtr hChannelHandle,
            VirtualChannel wtsvirtualclass,
            IntPtr ppBuffer,
            IntPtr pBytesReturned);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSRegisterSessionNotification(IntPtr handle, long dwFlags);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSUnRegisterSessionNotification(IntPtr handle);

        [DllImport(Dll.WtsApi32)]
        public static extern bool WTSQueryUserToken(IntPtr SessionId, IntPtr phToken);

        #endregion
#endif

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    #region Enumerations

    public static class HWND
    {
        public static readonly IntPtr Top = new IntPtr(0);
        public static readonly IntPtr Bottom = new IntPtr(1);
        public static readonly IntPtr TopMost = new IntPtr(-1);
        public static readonly IntPtr NoTopMost = new IntPtr(-2);

    }

    [Flags]
    public enum SetWindowPosFlags
    {
        NoSize = 0x0001,
        NoMove = 0x0002,
        NoZOrder = 0x0004,
        NoRedraw = 0x0008,
        NoActivate = 0x0010,
        FrameChanged = 0x0020,
        ShowWindow = 0x0040,
        HideWindow = 0x0080,
        NoCopyBits = 0x0100,
        NoOwnerZOrder = 0x0200,
        NoSendChanging = 0x0400,
        DeferErase = 0x2000,
        AsyncWindowPos = 0x4000,
    }

    public enum ComputerJoinStatus : uint
    {
        Unknown = 0,
        Unjoined,
        Workgroup,
        Domain,
    }

    public enum WtsSessionInfoClass : uint
    {
        InitialProgram,
        ApplicationName,
        WorkingDirectory,
        OEMId,
        SessionId,
        UserName,
        WinStationName,
        DomainName,
        ConnectState,
        ClientBuildNumber,
        ClientName,
        ClientDirectory,
        ClientProductId,
        ClientHardwareId,
        ClientAddress,
        ClientDisplay,
        ClientProtocolType,
    }

    public enum WtsShutdownType : uint
    {
        Logoff = 1,      // log off all users except
        // current user; deletes WinStations (a reboot is
        // required to recreate the WinStations)
        Shutdown = 2,    // shutdown system
        Reboot = 4,      // shutdown and reboot
        PowerOff = 8,    // shutdown and power off (on machines that support power off through software)
        FastReboot = 16, // reboot without logging users off or shutting down
    }


    public enum ReparsePointType : uint
    {
        MountPoint = 0xA0000003,
        SymbolicLink = 0xA000000C,
        HierarchicalStorage = 0xC0000004,
        SingleInstanceStore = 0x80000007,
        DistributedFileSystem = 0x8000000A,
        FilterManager = 0x8000000B,
    }


    [Flags]
    public enum TokenAccessLevels
    {
        AssignPrimary = 0x00000001,
        Duplicate = 0x00000002,
        Impersonate = 0x00000004,
        Query = 0x00000008,
        QuerySource = 0x00000010,
        AdjustPrivileges = 0x00000020,
        AdjustGroups = 0x00000040,
        AdjustDefault = 0x00000080,
        AdjustSessionId = 0x00000100,

        Read = 0x00020000 | Query,

        Write = 0x00020000 | AdjustPrivileges | AdjustGroups | AdjustDefault,

        AllAccess = 0x000F0000 |
            AssignPrimary |
            Duplicate |
            Impersonate |
            Query |
            QuerySource |
            AdjustPrivileges |
            AdjustGroups |
            AdjustDefault |
            AdjustSessionId,

        MaximumAllowed = 0x02000000
    }

    public enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin
    }

    public enum LogonTypes : uint
    {
        Interactive = 2,
        Network,
        Batch,
        Service,
        NetworkCleartext = 8,
        NewCredentials
    }

    public enum LogonProviders : uint
    {
        Default = 0, // default for platform (use this!)
        WinNT35,     // sends smoke signals to authority
        WinNT40,     // uses NTLM
        WinNT50      // negotiates Kerb or NTLM
    }

    [Flags]
    public enum PrivilegeStatus : uint
    {
        Disabled = 0,
        EnabledByDefault = 0x00000001,
        Enabled = 0x00000002,
        UsedForAccess = 0x80000000

    }

    /// <summary>
    /// <see cref="IAssemblyName.GetDisplayName"/>
    /// </summary>
    [Flags]
    public enum AssemblyDisplayFlags
    {
        Version = 0x1,
        Culture = 0x2,
        PublicKeyToken = 0x4,
        PublicKey = 0x8,
        Custom = 0x10,
        ProcessorArchitecture = 0x20,
        LanguageId = 0x40
    }

    [Flags]
    public enum AssemblyCompareFlags
    {
        Name = 0x1,
        MajorVersion = 0x2,
        MinorVersion = 0x4,
        BuildNumber = 0x8,
        RevisionNumber = 0x10,
        PublicKeyToken = 0x20,
        Culture = 0x40,
        Custom = 0x80,
        All = Name | MajorVersion | MinorVersion |
            RevisionNumber | BuildNumber |
            PublicKeyToken | Culture | Custom,
        Default = 0x100
    }

    /// <summary>
    /// The ASM_NAME enumeration property ID describes the valid names of the name-value pairs in an assembly name. 
    /// See the .NET Framework SDK for a description of these properties. 
    /// </summary>
    public enum AssemblyNameProperty
    {
        PublicKey = 0,
        PublicKeyToken,
        Hash,
        Name,
        MajorVersion,
        MinorVersion,
        BuildNumber,
        RevisionNumber,
        Culture,
        ProcessorIdArray,
        OSInfoArray,
        HashAlgorithm,
        Alias,
        CodebaseUrl,
        CodebaseLastModified,
        NullPublicKey,
        NullPublicKeyToken,
        Custom,
        NullCustom,
        Mvid,
        MaxParams
    }

    /// <summary>
    /// <see cref="IAssemblyCache.UninstallAssembly"/>
    /// </summary>
    public enum IASSEMBLYCACHE_UNINSTALL_DISPOSITION
    {
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED = 1,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE = 2,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED = 3,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING = 4,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES = 5,
        IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND = 6
    }

    /// <summary>
    /// <see cref="IAssemblyCache.QueryAssemblyInfo"/>
    /// </summary>
    public enum QueryAsmInfoFlag
    {
        Validate = 1,
        GetSize = 2
    }

    /// <summary>
    /// <see cref="IAssemblyChance.InstallAssembly"/>
    /// </summary>
    public enum IASSEMBLYCACHE_INSTALL_FLAG
    {
        IASSEMBLYCACHE_INSTALL_FLAG_REFRESH = 1,
        IASSEMBLYCACHE_INSTALL_FLAG_FORCE_REFRESH = 2
    }

    /// <summary>
    /// The CREATE_ASM_NAME_OBJ_FLAGS enumeration contains the following values: 
    ///	CANOF_PARSE_DISPLAY_NAME - If this flag is specified, the szAssemblyName parameter is a full assembly name and is parsed to 
    ///		the individual properties. If the flag is not specified, szAssemblyName is the "Name" portion of the assembly name.
    ///	CANOF_SET_DEFAULT_VALUES - If this flag is specified, certain properties, such as processor architecture, are set to 
    ///		their default values.
    ///	<see cref="AssemblyCache.CreateAssemblyNameObject"/>
    /// </summary>
    [Flags]
    public enum CreateAssemblyNameFlags : uint
    {
        //CANOF_PARSE_DISPLAY_NAME = 0x1,
        //CANOF_SET_DEFAULT_VALUES = 0x2

        // oisin: added missing values and [Flags] attrib
        ParseDisplayName = 0x1,
        SetDefaultValues = 0x2, // current
        VerifyFriendAssemblyName = 0x4,
        ParseFriendAssemblyName =
            ParseDisplayName | VerifyFriendAssemblyName
    }

    /// <summary>
    /// The ASM_CACHE_FLAGS enumeration contains the following values: 
    /// ASM_CACHE_ZAP - Enumerates the cache of precompiled assemblies by using Ngen.exe.
    /// ASM_CACHE_GAC - Enumerates the GAC.
    /// ASM_CACHE_DOWNLOAD - Enumerates the assemblies that have been downloaded on-demand or that have been shadow-copied.
    /// </summary>
    [Flags]
    public enum AssemblyCacheType
    {
        NGen = 0x1,
        Gac = 0x2,
        Download = 0x4
    }

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Height
        {
            get { return (Bottom - Top); }
        }

        public int Width
        {
            get
            {
                return (Right - Left);
            }
        }

        public System.Drawing.Size Size
        {
            get { return new System.Drawing.Size(Width, Height); }
        }

        public System.Drawing.Point Location
        {
            get { return new Point(Left, Top); }
        }

        public System.Drawing.Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        public static RECT FromRectangle(Rectangle rectangle)
        {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public override int GetHashCode()
        {
            return (((Left ^ ((Top << 13) | (Top >> 0x13))) ^ ((Width << 0x1a) | (Width >> 6))) ^ ((Height << 7) | (Height >> 0x19)));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WtsClientAddress
    {
        public System.Net.Sockets.AddressFamily AddressFamily;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Address;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public PrivilegeStatus Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public ComTypes.FILETIME ftCreationTime;
        public ComTypes.FILETIME ftLastAccessTime;
        public ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct REPARSE_GUID_DATA_BUFFER
    {
        public ReparsePointType ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;

        public Guid ReparseGuid;
        public byte DataBuffer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct MOUNTPOINT_REPARSE_DATA_BUFFER
    {
        public ReparsePointType ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;

        // IO_REPARSE_TAG_MOUNT_POINT specifics follow
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;

        public char PathBuffer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct SET_MOUNTPOINT_REPARSE_DATA_BUFFER
    {
        public uint ReparseTag;
        public uint ReparseDataLength;
        public ushort Reserved;
        public ushort ReparseTargetLength;
        public ushort ReparseTargetMaximumLength;
        public ushort Reserved1;
        public char ReparseTarget;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct SYMLINK_REPARSE_DATA_BUFFER
    {
        public ReparsePointType ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;

        // IO_REPARSE_TAG_MOUNT_POINT specifics follow
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;

        public uint Reserved1;

        public char PathBuffer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WTS_SESSION_INFO
    {
        public int SessionId;
        public string WinStationName;
        public int State;
    }

    /// <summary>
    /// The FUSION_INSTALL_REFERENCE structure represents a reference that is made when an application has installed an 
    /// assembly in the GAC. 
    /// The fields of the structure are defined as follows: 
    ///		cbSize - The size of the structure in bytes.
    ///		dwFlags - Reserved, must be zero.
    ///		guidScheme - The entity that adds the reference.
    ///		szIdentifier - A unique string that identifies the application that installed the assembly.
    ///		szNonCannonicalData - A string that is only understood by the entity that adds the reference. 
    ///				The GAC only stores this string.
    /// Possible values for the guidScheme field can be one of the following: 
    ///		FUSION_REFCOUNT_MSI_GUID - The assembly is referenced by an application that has been installed by using 
    ///				Windows Installer. The szIdentifier field is set to MSI, and szNonCannonicalData is set to Windows Installer. 
    ///				This scheme must only be used by Windows Installer itself.
    ///		FUSION_REFCOUNT_UNINSTALL_SUBKEY_GUID - The assembly is referenced by an application that appears in Add/Remove 
    ///				Programs. The szIdentifier field is the token that is used to register the application with Add/Remove programs.
    ///		FUSION_REFCOUNT_FILEPATH_GUID - The assembly is referenced by an application that is represented by a file in 
    ///				the file system. The szIdentifier field is the path to this file.
    ///		FUSION_REFCOUNT_OPAQUE_STRING_GUID - The assembly is referenced by an application that is only represented 
    ///				by an opaque string. The szIdentifier is this opaque string. The GAC does not perform existence checking 
    ///				for opaque references when you remove this.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FUSION_INSTALL_REFERENCE
    {
        public uint cbSize;
        public uint dwFlags;
        public Guid guidScheme;
        public string szIdentifier;
        public string szNonCannonicalData;
    }

    /// <summary>
    /// The ASSEMBLY_INFO structure represents information about an assembly in the assembly cache. 
    /// The fields of the structure are defined as follows: 
    ///		cbAssemblyInfo - Size of the structure in bytes. Permits additions to the structure in future version of the .NET Framework.
    ///		dwAssemblyFlags - Indicates one or more of the ASSEMBLYINFO_FLAG_* bits.
    ///		uliAssemblySizeInKB - The size of the files that make up the assembly in kilobytes (KB).
    ///		pszCurrentAssemblyPathBuf - A pointer to a string buffer that holds the current path of the directory that contains the 
    ///				files that make up the assembly. The path must end with a zero.
    ///		cchBuf - Size of the buffer that the pszCurrentAssemblyPathBug field points to.
    ///	dwAssemblyFlags can have one of the following values: 
    ///		ASSEMBLYINFO_FLAG__INSTALLED - Indicates that the assembly is actually installed. Always set in current version of the 
    ///				.NET Framework.
    ///		ASSEMBLYINFO_FLAG__PAYLOADRESIDENT - Never set in the current version of the .NET Framework.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct AssemblyInfo
    {
        public int cbAssemblyInfo;
        public int dwAssemblyFlags;
        public ulong uliAssemblySizeInKB;
        public string pszCurrentAssemblyPathBuf;
        public int cchBuf;

        public static AssemblyInfo Create()
        {
            String buffer = new String('\0', NativeMethods.MAX_PATH);
            AssemblyInfo info = new AssemblyInfo();

            info.cbAssemblyInfo = Marshal.SizeOf(info);
            info.pszCurrentAssemblyPathBuf = buffer;
            info.cchBuf = buffer.Length;

            return info;
        }
    }

    #endregion

    #region SafeHandles

    public class SafeFindVolumeMountPointHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFindVolumeMountPointHandle()
            : base(true)
        {
        }
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FindVolumeMountPointClose(handle);
        }
    }
    #endregion
}
