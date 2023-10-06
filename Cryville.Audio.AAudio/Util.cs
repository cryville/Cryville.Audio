using Android.AAudio.Native;
using System;

namespace Cryville.Audio.AAudio {
	internal static class Util {
		public static SampleFormat FromInternalSampleFormat(aaudio_format_t sampleFormat) {
			switch (sampleFormat) {
				case aaudio_format_t.AAUDIO_FORMAT_PCM_I16: return SampleFormat.S16;
				case aaudio_format_t.AAUDIO_FORMAT_PCM_FLOAT: return SampleFormat.F32;
				case aaudio_format_t.AAUDIO_FORMAT_PCM_I24_PACKED: return SampleFormat.S24;
				case aaudio_format_t.AAUDIO_FORMAT_PCM_I32: return SampleFormat.S32;
				default: return SampleFormat.Invalid;
			}
		}

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

		public static aaudio_direction_t ToInternalDataFlow(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return aaudio_direction_t.AAUDIO_DIRECTION_OUTPUT;
				case DataFlow.In: return aaudio_direction_t.AAUDIO_DIRECTION_INPUT;
				default: throw new ArgumentOutOfRangeException(nameof(dataFlow));
			}
		}

		public static aaudio_format_t ToInternalSampleFormat(SampleFormat sampleFormat) {
			switch (sampleFormat) {
				case SampleFormat.S16: return aaudio_format_t.AAUDIO_FORMAT_PCM_I16;
				case SampleFormat.F32: return aaudio_format_t.AAUDIO_FORMAT_PCM_FLOAT;
				case SampleFormat.S24: return aaudio_format_t.AAUDIO_FORMAT_PCM_I24_PACKED;
				case SampleFormat.S32: return aaudio_format_t.AAUDIO_FORMAT_PCM_I32;
				default: throw new NotSupportedException("Unsupported sample format.");
			}
		}

		public static aaudio_sharing_mode_t ToInternalSharingMode(AudioShareMode shareMode) {
			switch (shareMode) {
				case AudioShareMode.Shared: return aaudio_sharing_mode_t.AAUDIO_SHARING_MODE_SHARED;
				case AudioShareMode.Exclusive: return aaudio_sharing_mode_t.AAUDIO_SHARING_MODE_EXCLUSIVE;
				default: throw new ArgumentOutOfRangeException(nameof(shareMode));
			}
		}
	}
}