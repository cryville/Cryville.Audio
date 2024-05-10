using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	internal enum SL_DATALOCATOR : UInt32 {
		NULL            = 0x00000000,
		URI             = 0x00000001,
		ADDRESS         = 0x00000002,
		IODEVICE        = 0x00000003,
		OUTPUTMIX       = 0x00000004,
		// RESERVED5       = 0x00000005,
		BUFFERQUEUE     = 0x00000006,
		MIDIBUFFERQUEUE = 0x00000007,
		MEDIAOBJECT     = 0x00000008,
		CONTENTPIPE     = 0x00000009,
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataLocator_BufferQueue(UInt32 num) {
		public UInt32 locatorType = (UInt32)SL_DATALOCATOR.BUFFERQUEUE;
		public UInt32 numBuffers = num;
	}
	[StructLayout(LayoutKind.Sequential)]
	internal struct SLDataLocator_OutputMix(IntPtr obj) {
		public UInt32 locatorType = (UInt32)SL_DATALOCATOR.OUTPUTMIX;
		public IntPtr outputMix = obj;
	}
}
