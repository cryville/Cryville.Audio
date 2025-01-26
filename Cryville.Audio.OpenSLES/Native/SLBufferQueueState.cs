using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSLES.Native {
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLBufferQueueState {
		public UInt32 count;
		public UInt32 playIndex;
	}
}