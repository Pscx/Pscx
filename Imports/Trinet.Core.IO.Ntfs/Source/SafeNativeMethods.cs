using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Trinet.Core.IO.Ntfs
{
	using Resources = Trinet.Core.IO.Ntfs.Properties.Resources;

	/// <summary>
	/// Safe native methods.
	/// </summary>
	internal static class SafeNativeMethods
	{
		#region Constants and flags

		public const int MaxPath = 256;
		private const string LongPathPrefix = @"\\?\";
		public const char StreamSeparator = ':';
		public const int DefaultBufferSize = 0x1000;

		private const int ErrorFileNotFound = 2;

		[Flags]
		public enum NativeFileFlags : uint
		{
			WriteThrough		= 0x80000000,
			Overlapped			= 0x40000000,
			NoBuffering			= 0x20000000,
			RandomAccess		= 0x10000000,
			SequentialScan		= 0x8000000,
			DeleteOnClose		= 0x4000000,
			BackupSemantics		= 0x2000000,
			PosixSemantics		= 0x1000000,
			OpenReparsePoint	= 0x200000,
			OpenNoRecall		= 0x100000
		}

		[Flags]
		public enum NativeFileAccess : uint
		{
			GenericRead		= 0x80000000,
			GenericWrite	= 0x40000000
		}

		#endregion

		#region P/Invoke Structures

		[StructLayout(LayoutKind.Sequential)]
		private struct LargeInteger
		{
			public int Low;
			public int High;

			public long ToInt64()
			{
				return (this.High * 0x100000000) + this.Low;
			}

			/*
			public static LargeInteger FromInt64(long value)
			{
				return new LargeInteger
				{
					Low = (int)(value & 0x11111111),
					High = (int)((value / 0x100000000) & 0x11111111)
				};
			}
			*/
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Win32StreamId
		{
			public int StreamId;
			public int StreamAttributes;
			public LargeInteger Size;
			public int StreamNameSize;
		}

/*
		[StructLayout(LayoutKind.Sequential)]
		private struct FileInformationByHandle
		{
			public int dwFileAttributes;
			public LargeInteger ftCreationTime;
			public LargeInteger ftLastAccessTime;
			public LargeInteger ftLastWriteTime;
			public int dwVolumeSerialNumber;
			public LargeInteger FileSize;
			public int nNumberOfLinks;
			public LargeInteger FileIndex;
		}
*/

		#endregion

		#region P/Invoke Methods

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int GetFileAttributes(string fileName);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetFileSizeEx(SafeFileHandle handle, out LargeInteger size);

		[DllImport("kernel32.dll")]
		private static extern int GetFileType(SafeFileHandle handle);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(
			string name,
			NativeFileAccess access,
			FileShare share,
			IntPtr security,
			FileMode mode,
			NativeFileFlags flags,
			IntPtr template);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteFile(string name);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupRead(
			SafeFileHandle hFile,
			ref Win32StreamId pBuffer,
			int numberOfBytesToRead,
			out int numberOfBytesRead,
			[MarshalAs(UnmanagedType.Bool)] bool abort,
			[MarshalAs(UnmanagedType.Bool)] bool processSecurity,
			ref IntPtr context);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupRead(
			SafeFileHandle hFile,
			SafeHGlobalHandle pBuffer,
			int numberOfBytesToRead,
			out int numberOfBytesRead,
			[MarshalAs(UnmanagedType.Bool)] bool abort,
			[MarshalAs(UnmanagedType.Bool)] bool processSecurity,
			ref IntPtr context);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupSeek(
			SafeFileHandle hFile,
			int bytesToSeekLow,
			int bytesToSeekHigh,
			out int bytesSeekedLow,
			out int bytesSeekedHigh,
			ref IntPtr context);

		#endregion

		#region Utility Structures

		public struct Win32StreamInfo
		{
			public FileStreamType StreamType;
			public FileStreamAttributes StreamAttributes;
			public long StreamSize;
			public string StreamName;
		}

		#endregion

		#region Utility Methods

		public static void ThrowLastWin32Error()
		{
			int error = Marshal.GetLastWin32Error();
			if (0 != error)
			{
				int hr = Marshal.GetHRForLastWin32Error();
				if (0 > hr) Marshal.ThrowExceptionForHR(error);
				throw new Win32Exception(error);
			}
		}

		public static NativeFileAccess ToNative(FileAccess access)
		{
			NativeFileAccess result = 0;
			if (FileAccess.Read == (FileAccess.Read & access)) result |= NativeFileAccess.GenericRead;
			if (FileAccess.Write == (FileAccess.Write & access)) result |= NativeFileAccess.GenericWrite;
			return result;
		}

		public static string BuildStreamPath(string filePath, string streamName)
		{
			string result = filePath;
			if (!string.IsNullOrEmpty(filePath))
			{
				if (1 == result.Length) result = ".\\" + result;
				result += StreamSeparator + streamName + StreamSeparator + "$DATA";
				if (MaxPath <= result.Length) result = LongPathPrefix + result;
			}
			return result;
		}

		public static void ValidateStreamName(string streamName)
		{
			if (!string.IsNullOrEmpty(streamName) && -1 != streamName.IndexOfAny(Path.GetInvalidFileNameChars()))
			{
				throw new ArgumentException(Resources.Error_InvalidFileChars);
			}
		}

		public static int SafeGetFileAttributes(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			int result = GetFileAttributes(name);
			if (-1 == result)
			{
				int errorCode = Marshal.GetLastWin32Error();
				if (ErrorFileNotFound != errorCode) ThrowLastWin32Error();
			}

			return result;
		}

		public static bool SafeDeleteFile(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			bool result = DeleteFile(name);
			if (!result)
			{
				int errorCode = Marshal.GetLastWin32Error();
				if (ErrorFileNotFound != errorCode) ThrowLastWin32Error();
			}

			return result;
		}

		public static SafeFileHandle SafeCreateFile(string path, NativeFileAccess access, FileShare share, IntPtr security, FileMode mode, NativeFileFlags flags, IntPtr template)
		{
			SafeFileHandle result = CreateFile(path, access, share, security, mode, flags, template);
			if (!result.IsInvalid && 1 != GetFileType(result))
			{
				result.Dispose();
				throw new NotSupportedException(string.Format(Resources.Culture,
					Resources.Error_NonFile, path));
			}

			return result;
		}

		public static long GetFileSize(SafeFileHandle handle)
		{
			long result = 0L;
			if (null != handle && !handle.IsInvalid)
			{
				LargeInteger value;
				if (GetFileSizeEx(handle, out value))
				{
					result = value.ToInt64();
				}
				else
				{
					ThrowLastWin32Error();
				}
			}

			return result;
		}

		public static long GetFileSize(string path)
		{
			long result = 0L;
			if (!string.IsNullOrEmpty(path))
			{
				using (SafeFileHandle handle = SafeCreateFile(path, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
				{
					result = GetFileSize(handle);
				}
			}

			return result;
		}

		public static IList<Win32StreamInfo> ListStreams(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			if (-1 != filePath.IndexOfAny(Path.GetInvalidPathChars())) throw new ArgumentException(Resources.Error_InvalidFileChars, "filePath");

			List<Win32StreamInfo> result = new List<Win32StreamInfo>();

			using (SafeFileHandle hFile = SafeCreateFile(filePath, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, NativeFileFlags.BackupSemantics, IntPtr.Zero))
			using (StreamName hName = new StreamName())
			{
				if (!hFile.IsInvalid)
				{
					Win32StreamId streamId = new Win32StreamId();
					int dwStreamHeaderSize = Marshal.SizeOf(streamId);
					bool finished = false;
					IntPtr context = IntPtr.Zero;
					int bytesRead;
					string name;

					try
					{
						while (!finished)
						{
							// Read the next stream header:
							if (!BackupRead(hFile, ref streamId, dwStreamHeaderSize, out bytesRead, false, false, ref context))
							{
								finished = true;
							}
							else if (dwStreamHeaderSize != bytesRead)
							{
								finished = true;
							}
							else
							{
								// Read the stream name:
								if (0 >= streamId.StreamNameSize)
								{
									name = null;
								}
								else
								{
									hName.EnsureCapacity(streamId.StreamNameSize);
									if (!BackupRead(hFile, hName.MemoryBlock, streamId.StreamNameSize, out bytesRead, false, false, ref context))
									{
										name = null;
										finished = true;
									}
									else
									{
										// Unicode chars are 2 bytes:
										name = hName.ReadStreamName(bytesRead >> 1);
									}
								}

								// Add the stream info to the result:
								if (!string.IsNullOrEmpty(name))
								{
									result.Add(new Win32StreamInfo
									{
										StreamType = (FileStreamType)streamId.StreamId,
										StreamAttributes = (FileStreamAttributes)streamId.StreamAttributes,
										StreamSize = streamId.Size.ToInt64(),
										StreamName = name
									});
								}

								// Skip the contents of the stream:
								int bytesSeekedLow, bytesSeekedHigh;
								if (!finished && !BackupSeek(hFile, streamId.Size.Low, streamId.Size.High, out bytesSeekedLow, out bytesSeekedHigh, ref context))
								{
									finished = true;
								}
							}
						}
					}
					finally
					{
						// Abort the backup:
						BackupRead(hFile, hName.MemoryBlock, 0, out bytesRead, true, false, ref context);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
