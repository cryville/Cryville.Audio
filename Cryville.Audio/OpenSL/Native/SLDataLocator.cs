using System;
using System.Runtime.InteropServices;

namespace OpenSL.Native {
	public enum SL_DATALOCATOR : UInt32 {
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
#pragma warning disable CA1711
	[StructLayout(LayoutKind.Sequential)]
	public struct SLDataLocator_BufferQueue {
		public UInt32 locatorType;
		public UInt32 numBuffers;
		public SLDataLocator_BufferQueue(UInt32 num) {
			locatorType = (uint)SL_DATALOCATOR.BUFFERQUEUE;
			numBuffers = num;
		}
	}
#pragma warning restore CA1711
	[StructLayout(LayoutKind.Sequential)]
	public struct SLDataLocator_OutputMix {
		public UInt32 locatorType;
		public IntPtr outputMix;
		public SLDataLocator_OutputMix(IntPtr obj) {
			locatorType = (uint)SL_DATALOCATOR.OUTPUTMIX;
			outputMix = obj;
		}
	}
}
