using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.AudioClient {
	internal static class IAudioClient {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_Initialize")]
		public static extern void Initialize(
			IntPtr self,
			AUDCLNT_SHAREMODE ShareMode,
			UInt32 StreamFlags,
			Int64 hnsBufferDuration,
			Int64 hnsPeriodicity,
			ref WAVEFORMATEX pFormat,
			/* ref Guid */ IntPtr AudioSessionGuid
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetBufferSize")]
		public static extern void GetBufferSize(
			IntPtr self,
			out UInt32 pNumBufferFrames
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetStreamLatency")]
		public static extern void GetStreamLatency(
			IntPtr self,
			out Int64 phnsLatency
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetCurrentPadding")]
		public static extern void GetCurrentPadding(
			IntPtr self,
			out UInt32 pNumPaddingFrames
		);

		[PreserveSig]
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_IsFormatSupported")]
		public static extern int IsFormatSupported(
			IntPtr self,
			AUDCLNT_SHAREMODE ShareMode,
			ref WAVEFORMATEX pFormat,
			/* out *WAVEFORMATEX */ out IntPtr ppClosestMatch
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetMixFormat")]
		public static extern void GetMixFormat(
			IntPtr self,/* out *WAVEFORMATEX */ out IntPtr ppDeviceFormat);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetDevicePeriod")]
		public static extern void GetDevicePeriod(
			IntPtr self,
			out Int64 phnsDefaultDevicePeriod,
			out Int64 phnsMinimumDevicePeriod
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_Start")]
		public static extern void Start(
			IntPtr self
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_Stop")]
		public static extern void Stop(
			IntPtr self
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_Reset")]
		public static extern void Reset(
			IntPtr self
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_SetEventHandle")]
		public static extern void SetEventHandle(
			IntPtr self,
			IntPtr eventHandle
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClient_GetService")]
		public static extern void GetService(
			IntPtr self,
			ref Guid riid,
			out IntPtr ppv
		);
	}

	internal static class IAudioClock {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClock_GetFrequency")]
		public static extern void GetFrequency(
			IntPtr self,
			out UInt64 pu64Frequency
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClock_GetPosition")]
		public static extern void GetPosition(
			IntPtr self,
			out UInt64 pu64Position,
			out UInt64 pu64QPCPosition
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioClock_GetCharacteristics")]
		public static extern void GetCharacteristics(
			IntPtr self,
			out UInt32 pdwCharacteristics
		);
	}

	internal static class IAudioRenderClient {
		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioRenderClient_GetBuffer")]
		public static extern void GetBuffer(
			IntPtr self,
			UInt32 NumFramesRequested,
			out IntPtr ppData
		);

		[DllImport("Cryville.Audio.WasapiWrapper.dll", EntryPoint = "IAudioRenderClient_ReleaseBuffer")]
		public static extern void ReleaseBuffer(
			IntPtr self,
			UInt32 NumFramesWritten,
			UInt32 dwFlags
		);
	}

	[Flags]
	internal enum AUDCLNT_BUFFERFLAGS : UInt32 {
		DATA_DISCONTINUITY = 0x1,
		SILENT             = 0x2,
		TIMESTAMP_ERROR    = 0x4,
	};

	[Flags]
	internal enum DEVICE_STATE_XXX : UInt32 {
		DEVICE_STATE_ACTIVE     = 0x00000001,
		DEVICE_STATE_DISABLED   = 0x00000002,
		DEVICE_STATE_NOTPRESENT = 0x00000004,
		DEVICE_STATE_UNPLUGGED  = 0x00000008,
		DEVICE_STATEMASK_ALL    = 0x0000000F,
	}
}
