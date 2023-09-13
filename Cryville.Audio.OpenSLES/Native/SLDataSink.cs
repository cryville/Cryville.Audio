using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataSink {
		public IntPtr pLocator;
		public IntPtr pFormat;
		public SLDataSink(IntPtr locator, IntPtr format) {
			pLocator = locator;
			pFormat = format;
		}
	}
}