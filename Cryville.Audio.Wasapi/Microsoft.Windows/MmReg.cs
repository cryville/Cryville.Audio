using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.MmReg {
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	struct WAVEFORMATEXTENSIBLE {
		public WAVEFORMATEX Format;
		public ushort Samples;
		public int dwChannelMask;
		public Guid SubFormat;
	}

	internal enum WAVE_FORMAT : UInt16 {
		UNKNOWN = 0x0000,
		EXTENSIBLE = 0xFFFE,
		PCM = 1,
	}
}