using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.AudioClient {
	enum AUDCLNT_STREAMOPTIONS {
		NONE = 0x00,
		RAW = 0x01,
		MATCH_FORMAT = 0x02,
		AMBISONICS = 0x04
	}

	unsafe struct AudioClientProperties {
		public readonly UInt32 cbSize = (uint)sizeof(AudioClientProperties);
		public bool bIsOffload;
		public AUDIO_STREAM_CATEGORY eCategory;
		public AUDCLNT_STREAMOPTIONS Options;

		public AudioClientProperties() { }
	}

	[ComImport]
	[Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioClient {
		void Initialize(
			AUDCLNT_SHAREMODE ShareMode,
			UInt32 StreamFlags,
			Int64 hnsBufferDuration,
			Int64 hnsPeriodicity,
			ref WAVEFORMATEX pFormat,
			/* ref Guid */ IntPtr AudioSessionGuid
		);

		void GetBufferSize(
			out UInt32 pNumBufferFrames
		);

		void GetStreamLatency(
			out Int64 phnsLatency
		);

		void GetCurrentPadding(
			out UInt32 pNumPaddingFrames
		);

		[PreserveSig]
		int IsFormatSupported(
			AUDCLNT_SHAREMODE ShareMode,
			ref WAVEFORMATEX pFormat,
			/* out *WAVEFORMATEX */ out IntPtr ppClosestMatch
		);

		void GetMixFormat(/* out *WAVEFORMATEX */ out IntPtr ppDeviceFormat);

		void GetDevicePeriod(
			out Int64 phnsDefaultDevicePeriod,
			out Int64 phnsMinimumDevicePeriod
		);

		void Start();

		void Stop();

		void Reset();

		void SetEventHandle(
			IntPtr eventHandle
		);

		void GetService(
			[MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
	}

	[ComImport]
	[Guid("726778CD-F60A-4eda-82DE-E47610CD78AA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioClient2 : IAudioClient {
		void IsOffloadCapable(
			AUDIO_STREAM_CATEGORY Category,
			out bool pbOffloadCapable
		);

		void SetClientProperties(ref AudioClientProperties pProperties);

		void GetBufferSizeLimits(
			ref WAVEFORMATEX pFormat,
			bool bEventDriven,
			out Int64 phnsMinBufferDuration,
			out Int64 phnsMaxBufferDuration
		);
	}

	[ComImport]
	[Guid("CD63314F-3FBA-4a1b-812C-EF96358728E7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioClock {
		void GetFrequency(
			out UInt64 pu64Frequency
		);

		void GetPosition(
			out UInt64 pu64Position,
			out UInt64 pu64QPCPosition
		);

		void GetCharacteristics(
			out UInt32 pdwCharacteristics
		);
	}

	[ComImport]
	[Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioRenderClient {
		void GetBuffer(
			UInt32 NumFramesRequested,
			out IntPtr ppData
		);

		void ReleaseBuffer(
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
