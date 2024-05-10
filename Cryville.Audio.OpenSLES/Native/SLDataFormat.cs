using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	internal enum SL_DATAFORMAT : UInt32 {
		MIME = 0x00000001,
		PCM = 0x00000002,
	}
	internal enum SL_BYTEORDER : UInt32 {
		BIGENDIAN = 0x00000001,
		LITTLEENDIAN = 0x00000002,
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataFormat_PCM(UInt32 numChannels, UInt32 samplesPerSec, UInt32 bitsPerSample, UInt32 containerSize, UInt32 channelMask, UInt32 endianness) {
		public UInt32 formatType = (UInt32)SL_DATAFORMAT.PCM;
		public UInt32 numChannels = numChannels;
		public UInt32 samplesPerSec = samplesPerSec;
		public UInt32 bitsPerSample = bitsPerSample;
		public UInt32 containerSize = containerSize;
		public UInt32 channelMask = channelMask;
		public UInt32 endianness = endianness;
	}
}
