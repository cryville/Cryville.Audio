using Cryville.Common.Interop;
using System;
using System.Runtime.InteropServices;

namespace OpenSLES.Native {
	[StructLayout(LayoutKind.Sequential)]
	public struct SLAudioInputDescriptor {
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(LPUTF8StrMarshaler))] public string pDeviceName;
		public Int16 deviceConnection;
		public Int16 deviceScope;
		public Int16 deviceLocation;
		[MarshalAs(UnmanagedType.Bool)] public bool isForTelephony;
		public UInt32 minSampleRate;
		public UInt32 maxSampleRate;
		[MarshalAs(UnmanagedType.Bool)] public bool isFreqRangeContinuous;
		public IntPtr pSamplingRatesSupported;
		public Int16 numOfSamplingRatesSupported;
		public Int16 maxChannels;
	}
}