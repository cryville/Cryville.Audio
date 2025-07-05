using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.Mme {
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct WAVEFORMATEX {
		public UInt16 wFormatTag;      /* format type */
		public UInt16 nChannels;       /* number of channels (i.e. mono, stereo...) */
		public UInt32 nSamplesPerSec;  /* sample rate */
		public UInt32 nAvgBytesPerSec; /* for buffer estimation */
		public UInt16 nBlockAlign;     /* block size of data */
		public UInt16 wBitsPerSample;  /* number of bits per sample of mono data */
		public UInt16 cbSize;          /* the count in bytes of the size of */
		/* extra information (after cbSize) */
	}
}
