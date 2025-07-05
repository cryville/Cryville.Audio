using Cryville.Audio.AAudio.Native;
using Cryville.Interop.Java.Helper;
using System;
using System.Globalization;

namespace Cryville.Audio.AAudio {
	internal static class Helpers {
		public static SampleFormat FromInternalSampleFormat(aaudio_format_t sampleFormat) => sampleFormat switch {
			aaudio_format_t.AAUDIO_FORMAT_PCM_I16 => SampleFormat.S16,
			aaudio_format_t.AAUDIO_FORMAT_PCM_FLOAT => SampleFormat.F32,
			aaudio_format_t.AAUDIO_FORMAT_PCM_I24_PACKED => SampleFormat.S24,
			aaudio_format_t.AAUDIO_FORMAT_PCM_I32 => SampleFormat.S32,
			_ => SampleFormat.Invalid,
		};

		public static WaveFormat FromInternalWaveFormat(IntPtr stream) {
			var ch = UnsafeNativeMethods.AAudioStream_getChannelCount(stream);
			var cm = (ChannelMask)0;
			if (AndroidHelper.DeviceApiLevel >= 32) {
				try {
					cm = (ChannelMask)((int)UnsafeNativeMethods.AAudioStream_getChannelMask(stream) & 0x02ffffff);
				}
				catch (EntryPointNotFoundException) { }
			}
			var fmt = UnsafeNativeMethods.AAudioStream_getFormat(stream);
			var sr = UnsafeNativeMethods.AAudioStream_getSampleRate(stream);
			return new WaveFormat {
				Channels = (ushort)ch,
				SampleFormat = FromInternalSampleFormat(fmt),
				SampleRate = (uint)sr,
				ChannelMask = cm,
			};
		}

		public static void SetWaveFormatUsageAndShareMode(IntPtr builder, WaveFormat format, AudioUsage usage, AudioShareMode shareMode) {
			UnsafeNativeMethods.AAudioStreamBuilder_setChannelCount(builder, format.Channels);
			if (AndroidHelper.DeviceApiLevel >= 32) {
				if (format.ChannelMask != 0) {
					if (!format.IsChannelMaskValid()) throw new ArgumentException("Invalid channel mask.", nameof(format));
					try {
						UnsafeNativeMethods.AAudioStreamBuilder_setChannelMask(builder, (aaudio_channel_mask_t)((int)format.ChannelMask & 0x02ffffff));
					}
					catch (EntryPointNotFoundException) { }
				}
			}
			UnsafeNativeMethods.AAudioStreamBuilder_setFormat(builder, ToInternalSampleFormat(format.SampleFormat));
			UnsafeNativeMethods.AAudioStreamBuilder_setSampleRate(builder, (int)format.SampleRate);
			if (AndroidHelper.DeviceApiLevel >= 28) {
				try {
					UnsafeNativeMethods.AAudioStreamBuilder_setUsage(builder, ToInternalUsage(usage));
				}
				catch (EntryPointNotFoundException) { }
			}
			UnsafeNativeMethods.AAudioStreamBuilder_setSharingMode(builder, ToInternalSharingMode(shareMode));
		}

		public static aaudio_direction_t ToInternalDataFlow(DataFlow dataFlow) => dataFlow switch {
			DataFlow.Out => aaudio_direction_t.AAUDIO_DIRECTION_OUTPUT,
			DataFlow.In => aaudio_direction_t.AAUDIO_DIRECTION_INPUT,
			_ => throw new ArgumentOutOfRangeException(nameof(dataFlow)),
		};

		public static aaudio_format_t ToInternalSampleFormat(SampleFormat sampleFormat) => sampleFormat switch {
			SampleFormat.S16 => aaudio_format_t.AAUDIO_FORMAT_PCM_I16,
			SampleFormat.F32 => aaudio_format_t.AAUDIO_FORMAT_PCM_FLOAT,
			SampleFormat.S24 => aaudio_format_t.AAUDIO_FORMAT_PCM_I24_PACKED,
			SampleFormat.S32 => aaudio_format_t.AAUDIO_FORMAT_PCM_I32,
			_ => throw new NotSupportedException("Unsupported sample format."),
		};

		public static aaudio_sharing_mode_t ToInternalSharingMode(AudioShareMode shareMode) => shareMode switch {
			AudioShareMode.Shared => aaudio_sharing_mode_t.AAUDIO_SHARING_MODE_SHARED,
			AudioShareMode.Exclusive => aaudio_sharing_mode_t.AAUDIO_SHARING_MODE_EXCLUSIVE,
			_ => throw new ArgumentOutOfRangeException(nameof(shareMode)),
		};

		public static aaudio_usage_t ToInternalUsage(AudioUsage usage) => usage switch {
			AudioUsage.Media => aaudio_usage_t.AAUDIO_USAGE_MEDIA,
			AudioUsage.Communication => aaudio_usage_t.AAUDIO_USAGE_VOICE_COMMUNICATION,
			AudioUsage.Alarm => aaudio_usage_t.AAUDIO_USAGE_ALARM,
			AudioUsage.Notification => aaudio_usage_t.AAUDIO_USAGE_NOTIFICATION,
			AudioUsage.NotificationRingtone => aaudio_usage_t.AAUDIO_USAGE_NOTIFICATION_RINGTONE,
			AudioUsage.NotificationEvent => aaudio_usage_t.AAUDIO_USAGE_NOTIFICATION_EVENT,
			AudioUsage.AssistanceAccessibility => aaudio_usage_t.AAUDIO_USAGE_ASSISTANCE_ACCESSIBILITY,
			AudioUsage.AssistanceNavigation => aaudio_usage_t.AAUDIO_USAGE_ASSISTANCE_NAVIGATION_GUIDANCE,
			AudioUsage.AssistanceSonification => aaudio_usage_t.AAUDIO_USAGE_ASSISTANCE_SONIFICATION,
			AudioUsage.Game => aaudio_usage_t.AAUDIO_USAGE_GAME,
			_ => aaudio_usage_t.AAUDIO_USAGE_MEDIA,
		};

		public static void ThrowIfError(aaudio_result_t result) {
			if (result == aaudio_result_t.AAUDIO_OK) return;
			if (result == aaudio_result_t.AAUDIO_ERROR_DISCONNECTED) throw new AudioClientDisconnectedException();
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "AAudio error: {0}", result));
		}
	}
}