using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	internal enum SL_DATAFORMAT : UInt32 {
		MIME = 0x00000001,
		PCM = 0x00000002,
	}
	internal enum SL_BYTEORDER : UInt32 {
		BIGENDIAN = 0x00000001,
		LITTLEENDIAN = 0x00000002,
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataFormat_PCM {
		public UInt32 formatType;
		public UInt32 numChannels;
		public UInt32 samplesPerSec;
		public UInt32 bitsPerSample;
		public UInt32 containerSize;
		public UInt32 channelMask;
		public UInt32 endianness;

		public SLDataFormat_PCM(uint numChannels, uint samplesPerSec, uint bitsPerSample, uint containerSize, uint channelMask, uint endianness) {
			formatType = (UInt32)SL_DATAFORMAT.PCM;
			this.numChannels = numChannels;
			this.samplesPerSec = samplesPerSec;
			this.bitsPerSample = bitsPerSample;
			this.containerSize = containerSize;
			this.channelMask = channelMask;
			this.endianness = endianness;
		}
	}
}
