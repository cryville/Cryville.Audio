using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows {
	internal static class Handle {
		[DllImport("kernel32.dll")][return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}
