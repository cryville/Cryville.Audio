using Cryville.Audio.OpenSLES.Native;
using System;

namespace Cryville.Audio.OpenSLES {
	internal static class Helpers {
		public static void SLR(SLResult result, string? desc = null) {
			if (result == SLResult.SUCCESS) return;
			var msg = result.ToString();
			if (desc != null) msg = desc + ": " + msg;
			throw new OpenSLException(msg);
		}

		public static SLDataFormat_PCM ToInternalFormat(WaveFormat value) {
			return new SLDataFormat_PCM(value.Channels, value.SampleRate * 1000, value.BitsPerSample, value.BitsPerSample, ~(0xffffffff << value.Channels), (UInt32)SL_BYTEORDER.LITTLEENDIAN);
		}
	}
}
