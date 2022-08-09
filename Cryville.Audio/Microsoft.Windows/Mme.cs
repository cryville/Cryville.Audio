using Microsoft.Windows.MmSysCom;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.Mme {
	[StructLayout(LayoutKind.Sequential)]
	public struct WAVEFORMATEX {
		public UInt16 wFormatTag;      /* format type */
		public UInt16 nChannels;       /* number of channels (i.e. mono, stereo...) */
		public UInt32 nSamplesPerSec;  /* sample rate */
		public UInt32 nAvgBytesPerSec; /* for buffer estimation */
		public UInt16 nBlockAlign;     /* block size of data */
		public UInt16 wBitsPerSample;  /* number of bits per sample of mono data */
		public UInt16 cbSize;          /* the count in bytes of the size of */
		/* extra information (after cbSize) */
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WAVEHDR {
		public IntPtr lpData;          /* pointer to locked data buffer */
		public UInt32 dwBufferLength;  /* length of data buffer */
		public UInt32 dwBytesRecorded; /* used for input only */
		public IntPtr dwUser;          /* for client's use */
		public UInt32 dwFlags;         /* assorted flags (see defines) */
		public UInt32 dwLoops;         /* loop control counter */
		public IntPtr lpNext;          /* reserved for driver */
		public IntPtr reserved;        /* reserved for driver */
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WAVEOUTCAPSW {
		public UInt16 wMid;           /* manufacturer ID */
		public UInt16 wPid;           /* product ID */
		public UInt32 vDriverVersion; /* version of the driver */
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MmSysComExports.MAXPNAMELEN)]
		public string szPname;        /* product name (NULL terminated string) */
		public UInt32 dwFormats;      /* formats supported */
		public UInt16 wChannels;      /* number of sources supported */
		public UInt16 wReserved1;     /* packing */
		public UInt32 dwSupport;      /* functionality supported by driver */
	}

	[Flags]
	public enum WAVE_FORMAT {
		INVALIDFORMAT = 0x00000000, /* invalid format */
		SR1M08  = 0x00000001,       /* 11.025 kHz, Mono,   8-bit  */
		SR1S08  = 0x00000002,       /* 11.025 kHz, Stereo, 8-bit  */
		SR1M16  = 0x00000004,       /* 11.025 kHz, Mono,   16-bit */
		SR1S16  = 0x00000008,       /* 11.025 kHz, Stereo, 16-bit */
		SR2M08  = 0x00000010,       /* 22.05  kHz, Mono,   8-bit  */
		SR2S08  = 0x00000020,       /* 22.05  kHz, Stereo, 8-bit  */
		SR2M16  = 0x00000040,       /* 22.05  kHz, Mono,   16-bit */
		SR2S16  = 0x00000080,       /* 22.05  kHz, Stereo, 16-bit */
		SR4M08  = 0x00000100,       /* 44.1   kHz, Mono,   8-bit  */
		SR4S08  = 0x00000200,       /* 44.1   kHz, Stereo, 8-bit  */
		SR4M16  = 0x00000400,       /* 44.1   kHz, Mono,   16-bit */
		SR4S16  = 0x00000800,       /* 44.1   kHz, Stereo, 16-bit */

		SR44M08 = 0x00000100,       /* 44.1   kHz, Mono,   8-bit  */
		SR44S08 = 0x00000200,       /* 44.1   kHz, Stereo, 8-bit  */
		SR44M16 = 0x00000400,       /* 44.1   kHz, Mono,   16-bit */
		SR44S16 = 0x00000800,       /* 44.1   kHz, Stereo, 16-bit */
		SR48M08 = 0x00001000,       /* 48     kHz, Mono,   8-bit  */
		SR48S08 = 0x00002000,       /* 48     kHz, Stereo, 8-bit  */
		SR48M16 = 0x00004000,       /* 48     kHz, Mono,   16-bit */
		SR48S16 = 0x00008000,       /* 48     kHz, Stereo, 16-bit */
		SR96M08 = 0x00010000,       /* 96     kHz, Mono,   8-bit  */
		SR96S08 = 0x00020000,       /* 96     kHz, Stereo, 8-bit  */
		SR96M16 = 0x00040000,       /* 96     kHz, Mono,   16-bit */
		SR96S16 = 0x00080000,       /* 96     kHz, Stereo, 16-bit */
	}

	[Flags]
	public enum WAVE_OPEN_FLAG {
		WAVE_FORMAT_QUERY                        = 0x0001,
		WAVE_ALLOWSYNC                           = 0x0002,
		WAVE_MAPPED                              = 0x0004,
		WAVE_FORMAT_DIRECT                       = 0x0008,
		WAVE_FORMAT_DIRECT_QUERY                 = (WAVE_FORMAT_QUERY | WAVE_FORMAT_DIRECT),
		WAVE_MAPPED_DEFAULT_COMMUNICATION_DEVICE = 0x0010,
	}

	[Flags]
	public enum WHDR {
		DONE      = 0x00000001, /* done bit */
		PREPARED  = 0x00000002, /* set if this header has been prepared */
		BEGINLOOP = 0x00000004, /* loop start block */
		ENDLOOP   = 0x00000008, /* loop end block */
		INQUEUE   = 0x00000010, /* reserved for driver */
	}

	public static class MmeExports {
		public const UInt32 WAVE_MAPPER = UInt32.MaxValue;

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutClose(IntPtr hwo);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutGetDevCapsW(
			UInt32 uDeviceID,
			out WAVEOUTCAPSW pwoc,
			UInt32 cbwoc
		);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutGetNumDevs();

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutGetPosition(
			IntPtr hwo,
			ref MMTIME pmmt,
			UInt32 cbmmt
		);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutOpen(
			ref IntPtr phwo,
			UInt32 uDeviceID,
			ref WAVEFORMATEX pwfx,
			IntPtr dwCallback,
			/* ref UInt32 */ IntPtr dwInstance,
			UInt32 fdwOpen
		);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutPause(IntPtr hwo);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutPrepareHeader(
			IntPtr hwo,
			ref WAVEHDR pwh,
			UInt32 cbwh
		);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutReset(IntPtr hwo);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutRestart(IntPtr hwo);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutUnprepareHeader(
			IntPtr hwo,
			ref WAVEHDR pwh,
			UInt32 cbwh
		);

		[DllImport("winmm.dll")]
		public static extern UInt32 waveOutWrite(
			IntPtr hwo,
			ref WAVEHDR pwh,
			UInt32 cbwh
		);
	}
}
