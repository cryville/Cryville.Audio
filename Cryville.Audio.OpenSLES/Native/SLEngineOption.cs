using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLEngineOption {
		public UInt32 feature;
		public UInt32 data;
	}
	internal enum SL_ENGINEOPTION : UInt32 {
		THREADSAFE    = 0x00000001,
		LOSSOFCONTROL = 0x00000002,
		MAJORVERSION  = 0x00000003,
		MINORVERSION  = 0x00000004,
		STEPVERSION   = 0x00000005,
	}
}