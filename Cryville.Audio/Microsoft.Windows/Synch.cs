using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows {
	public static class Synch {
		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateEventW(
			/* LPSECURITY_ATTRIBUTES */ IntPtr lpEventAttributes,
			[MarshalAs(UnmanagedType.Bool)] bool bManualReset,
			[MarshalAs(UnmanagedType.Bool)] bool bInitialState,
			[MarshalAs(UnmanagedType.LPWStr)] string lpName
		);

		[DllImport("kernel32.dll")]
		public static extern bool ResetEvent(IntPtr hEvent);

		[DllImport("kernel32.dll")]
		public static extern UInt32 WaitForSingleObject(
			IntPtr hHandle,
			UInt32 dwMilliseconds
		);
	}
}
