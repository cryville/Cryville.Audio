using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows {
	internal static class Synch {
#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateEventW(
			/* LPSECURITY_ATTRIBUTES */ IntPtr lpEventAttributes,
			[MarshalAs(UnmanagedType.Bool)] bool bManualReset,
			[MarshalAs(UnmanagedType.Bool)] bool bInitialState,
			[MarshalAs(UnmanagedType.LPWStr)] string lpName
		);

#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("kernel32.dll")]
		public static extern bool ResetEvent(IntPtr hEvent);

#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("kernel32.dll")]
		public static extern UInt32 WaitForSingleObject(
			IntPtr hHandle,
			UInt32 dwMilliseconds
		);
	}
}
