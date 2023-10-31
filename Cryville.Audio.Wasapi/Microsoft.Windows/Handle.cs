using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows {
	internal static class Handle {
#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}
