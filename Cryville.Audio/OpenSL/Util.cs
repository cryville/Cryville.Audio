using OpenSL.Native;
using System;
using System.Runtime.Serialization;

namespace Cryville.Audio.OpenSL {
	internal static class Util {
		public static void SLR(SLresult result, string desc = null) {
			if (result != SLresult.SUCCESS) {
				var msg = result.ToString();
				if (desc != null) msg = desc + ": " + msg;
				throw new OpenSLException(msg);
			}
		}

		public static SLDataFormat_PCM ToInternalFormat(WaveFormat value) {
			return new SLDataFormat_PCM(value.Channels, value.SampleRate * 1000, value.BitsPerSample, value.BitsPerSample, ~(0xffffffff << value.Channels), (UInt32)SL_BYTEORDER.LITTLEENDIAN);
		}
	}

	[Serializable]
	public class OpenSLException : Exception {
		public OpenSLException() { }
		public OpenSLException(string message) : base(message) { }
		public OpenSLException(string message, Exception innerException) : base(message, innerException) { }
		protected OpenSLException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
