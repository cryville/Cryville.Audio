using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Windows.MmSysCom {
	public static class MmSysComExports {
		public const int MAXPNAMELEN = 32;

		public static void MMR(uint ret) {
			var v = (MMSYSERR)ret;
			if (v != MMSYSERR.NOERROR)
				throw new MultimediaSystemException(v.ToString());
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct MMTIME {
		[FieldOffset(0)] public UInt32 wType;
		[FieldOffset(4)] private UInt32 ms;
		[FieldOffset(4)] private UInt32 sample;
		[FieldOffset(4)] private UInt32 cb;
		[FieldOffset(4)] private UInt32 ticks;
		[FieldOffset(4)] private smpte  smpte;
		[FieldOffset(4)] private UInt32 songptrpos;
		public object Value {
			get {
				switch ((TIME_TYPE)wType) {
					case TIME_TYPE.MS: return ms;
					case TIME_TYPE.SAMPLES: return sample;
					case TIME_TYPE.BYTES: return cb;
					case TIME_TYPE.TICKS: return ticks;
					case TIME_TYPE.SMPTE: return smpte;
					case TIME_TYPE.MIDI: return songptrpos;
					default: throw new NotSupportedException();
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct smpte {
		public byte hour;  /* hours */
		public byte min;   /* minutes */
		public byte sec;   /* seconds */
		public byte frame; /* frames  */
		public byte fps;   /* frames per second */
		byte dummy; /* pad */
		byte pad1;
		byte pad2;
	}

	public enum CALLBACK_TYPE {
		CALLBACK_TYPEMASK = 0x00070000,      /* callback type mask */
		CALLBACK_NULL     = 0x00000000,      /* no callback */
		CALLBACK_WINDOW   = 0x00010000,      /* dwCallback is a HWND */
		CALLBACK_TASK     = 0x00020000,      /* dwCallback is a HTASK */
		CALLBACK_FUNCTION = 0x00030000,      /* dwCallback is a FARPROC */
		CALLBACK_THREAD   = (CALLBACK_TASK), /* thread ID replaces 16 bit task */
		CALLBACK_EVENT    = 0x00050000,      /* dwCallback is an EVENT Handle */
	}

	public enum MMSYSERR {
		NOERROR = 0,
		ERROR,
		BADDEVICEID,
		NOTENABLED,
		ALLOCATED,
		INVALHANDLE,
		NODRIVER,
		NOMEM,
		NOTSUPPORTED,
		BADERRNUM,
		INVALFLAG,
		INVALPARAM,
		HANDLEBUSY,
		INVALIDALIAS,
		BADDB,
		KEYNOTFOUND,
		READERROR,
		WRITEERROR,
		DELETEERROR,
		VALNOTFOUND,
		NODRIVERCB,
		MOREDATA,
	}

	[Flags]
	public enum TIME_TYPE {
		MS      = 0x0001, /* time in milliseconds */
		SAMPLES = 0x0002, /* number of wave samples */
		BYTES   = 0x0004, /* current byte offset */
		SMPTE   = 0x0008, /* SMPTE time */
		MIDI    = 0x0010, /* MIDI time */
		TICKS   = 0x0020, /* Ticks within MIDI stream */
	}

		[Serializable]
	public class MultimediaSystemException : Exception {
		public MultimediaSystemException() { }
		public MultimediaSystemException(string message) : base(message) { }
		public MultimediaSystemException(string message, Exception innerException) : base(message, innerException) { }
		protected MultimediaSystemException(SerializationInfo serializationInfo, StreamingContext streamingContext) { }
	}
}
