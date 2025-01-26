using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataSink(IntPtr locator, IntPtr format) {
		public IntPtr pLocator = locator;
		public IntPtr pFormat = format;
	}
}