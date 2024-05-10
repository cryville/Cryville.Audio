using Android.AAudio.Native;
using System;

namespace Cryville.Audio.AAudio {
	internal static class Util {
		public static SampleFormat FromInternalSampleFormat(aaudio_format_t sampleFormat) => sampleFormat switch {
			aaudio_format_t.AAUDIO_FORMAT_PCM_I16 => SampleFormat.S16,
			aaudio_format_t.AAUDIO_FORMAT_PCM_FLOAT => SampleFormat.F32,
			aaudio_format_t.AAUDIO_FORMAT_PCM_I24_PACKED => SampleFormat.S24,
			aaudio_format_t.AAUDIO_FORMAT_PCM_I32 => SampleFormat.S32,
			_ => SampleFormat.Invalid,
		};

		public static WaveFormat FromInternalWaveFormat(IntPtr stream) {
			var ch = UnsafeNativeMethods.AAudioStream_getChannelCount(stream);
			var fmt = UnsafeNativeMethods.AAudioStream_getFormat(stream);
			var sr = UnsafeNativeMethods.AAudioStream_getSampleRate(stream);
			return new WaveFormat {
				Channels = (ushort)ch,
				SampleFormat = FromInternalSampleFormat(fmt),
				SampleRate = (uint)sr
			};
		}

		public static void SetWaveFormatAndShareMode(IntPtr builder, WaveFormat format, AudioShareMode shareMode) {
			UnsafeNativeMethods.AAudioStreamBuilder_setChannelCount(builder, format.Channels);
			UnsafeNativeMethods.AAudioStreamBuilder_setFormat(builder, ToInternalSampleFormat(format.SampleFormat));
			UnsafeNativeMethods.AAudioStreamBuilder_setSampleRate(builder, (int)format.SampleRate);
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
	}
}