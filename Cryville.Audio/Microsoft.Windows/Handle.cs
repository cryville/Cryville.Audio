using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Windows {
	public static class Handle {
		[DllImport("kernel32.dll")][return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}
