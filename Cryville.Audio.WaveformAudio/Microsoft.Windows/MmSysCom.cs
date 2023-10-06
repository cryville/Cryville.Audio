using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Windows.MmSysCom {
	internal static class MmSysComExports {
		public const int MAXPNAMELEN = 32;

		public static void MMR(uint ret) {
			var v = (MMSYSERR)ret;
			if (v != MMSYSERR.NOERROR)
				throw new MultimediaSystemException(v.ToString());
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct MMTIME {
		[FieldOffset(0)] public UInt32 wType;
		[FieldOffset(4)] readonly UInt32 ms;
		[FieldOffset(4)] readonly UInt32 sample;
		[FieldOffset(4)] readonly UInt32 cb;
		[FieldOffset(4)] readonly UInt32 ticks;
		[FieldOffset(4)] readonly smpte  smpte;
		[FieldOffset(4)] readonly UInt32 songptrpos;
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
	[SuppressMessage("Style", "IDE1006")]
	internal struct smpte {
		public byte hour;  /* hours */
		public byte min;   /* minutes */
		public byte sec;   /* seconds */
		public byte frame; /* frames  */
		public byte fps;   /* frames per second */
		readonly byte dummy; /* pad */
		readonly byte pad1;
		readonly byte pad2;
	}

	internal enum CALLBACK_TYPE {
		CALLBACK_TYPEMASK = 0x00070000,      /* callback type mask */
		CALLBACK_NULL     = 0x00000000,      /* no callback */
		CALLBACK_WINDOW   = 0x00010000,      /* dwCallback is a HWND */
		CALLBACK_TASK     = 0x00020000,      /* dwCallback is a HTASK */
		CALLBACK_FUNCTION = 0x00030000,      /* dwCallback is a FARPROC */
		CALLBACK_THREAD   = (CALLBACK_TASK), /* thread ID replaces 16 bit task */
		CALLBACK_EVENT    = 0x00050000,      /* dwCallback is an EVENT Handle */
	}

	internal enum MMSYSERR {
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
	internal enum TIME_TYPE {
		MS      = 0x0001, /* time in milliseconds */
		SAMPLES = 0x0002, /* number of wave samples */
		BYTES   = 0x0004, /* current byte offset */
		SMPTE   = 0x0008, /* SMPTE time */
		MIDI    = 0x0010, /* MIDI time */
		TICKS   = 0x0020, /* Ticks within MIDI stream */
	}

	/// <summary>
	/// Exception occurring in Multimedia System.
	/// </summary>
	[Serializable]
	public class MultimediaSystemException : Exception {
		/// <summary>
		/// Creates an instance of the <see cref="MultimediaSystemException" /> class.
		/// </summary>
		public MultimediaSystemException() { }
		/// <summary>
		/// Creates an instance of the <see cref="MultimediaSystemException" /> class.
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// </summary>
		public MultimediaSystemException(string message) : base(message) { }
		/// <summary>
		/// Creates an instance of the <see cref="MultimediaSystemException" /> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public MultimediaSystemException(string message, Exception innerException) : base(message, innerException) { }
		/// <summary>
		/// Creates an instance of the <see cref="MultimediaSystemException" /> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected MultimediaSystemException(SerializationInfo info, StreamingContext context) { }
	}
}
