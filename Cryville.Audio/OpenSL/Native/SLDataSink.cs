using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	[StructLayout(LayoutKind.Sequential)]
	public struct SLDataSink {
		public IntPtr pLocator;
		public IntPtr pFormat;
		public SLDataSink(IntPtr locator, IntPtr format) {
			pLocator = locator;
			pFormat = format;
		}
	}
}