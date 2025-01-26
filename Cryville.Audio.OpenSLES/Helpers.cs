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

		public static SL_ANDROID_STREAM ToInternalStreamType(AudioUsage usage) => usage switch {
			AudioUsage.Media or
			AudioUsage.Game => SL_ANDROID_STREAM.MEDIA,
			AudioUsage.Communication => SL_ANDROID_STREAM.VOICE,
			AudioUsage.Alarm => SL_ANDROID_STREAM.ALARM,
			AudioUsage.Notification or
			AudioUsage.NotificationEvent => SL_ANDROID_STREAM.NOTIFICATION,
			AudioUsage.NotificationRingtone => SL_ANDROID_STREAM.RING,
			_ => SL_ANDROID_STREAM.SYSTEM,
		};
	}
}
