using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	[StructLayout(LayoutKind.Sequential)]
	public struct SLDataSource {
		public IntPtr pLocator;
		public IntPtr pFormat;
		public SLDataSource(IntPtr locator, IntPtr format) {
			pLocator = locator;
			pFormat = format;
		}
	}
}