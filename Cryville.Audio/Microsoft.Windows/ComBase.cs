using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows {
	public static class ComBase {
		[DllImport("ole32.dll")]
		public static extern void CoCreateInstance(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
			IntPtr pUnkOuter,
			UInt32 dwClsContext,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
	}
}
