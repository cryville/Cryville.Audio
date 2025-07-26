using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.Mme;
using Microsoft.Windows.MmReg;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.Wasapi {
	internal static class Helpers {
		public static DataFlow FromInternalDataFlowEnum(EDataFlow value) => value switch {
			EDataFlow.eRender => DataFlow.Out,
			EDataFlow.eCapture => DataFlow.In,
			EDataFlow.eAll => DataFlow.All,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
		public static EDataFlow ToInternalDataFlowEnum(DataFlow value) => value switch {
			DataFlow.Out => EDataFlow.eRender,
			DataFlow.In => EDataFlow.eCapture,
			DataFlow.All => EDataFlow.eAll,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
		static readonly Guid _pcmSubtypeGuid = new("00000001-0000-0010-8000-00aa00389b71");
		static readonly Guid _floatSubtypeGuid = new("00000003-0000-0010-8000-00aa00389b71");
		public static WAVEFORMATEXTENSIBLE ToInternalFormat(WaveFormat value) {
			ushort blockAlign = (ushort)value.FrameSize;
			var result = new WAVEFORMATEX {
				wFormatTag = (ushort)WAVE_FORMAT.PCM,
				nChannels = value.Channels,
				nSamplesPerSec = value.SampleRate,
				nAvgBytesPerSec = value.SampleRate * blockAlign,
				nBlockAlign = blockAlign,
				wBitsPerSample = value.BitsPerSample,
				cbSize = 0,
			};
			if (value.Channels <= 2) return new WAVEFORMATEXTENSIBLE { Format = result };
			result.wFormatTag = (ushort)WAVE_FORMAT.EXTENSIBLE;
			result.cbSize = 22;
			int channelMask = (int)value.ChannelMask;
			int internalChannelMask = channelMask & 0x3ffff;
			if (internalChannelMask != channelMask) throw new PlatformNotSupportedException(string.Format(CultureInfo.InvariantCulture, "The channel mask {0} contains a channel that is not supported by WASAPI.", channelMask));
			return new WAVEFORMATEXTENSIBLE {
				Format = result,
				Samples = result.wBitsPerSample,
				dwChannelMask = internalChannelMask,
				SubFormat = value.SampleFormat is SampleFormat.F32 or SampleFormat.F64 ? _floatSubtypeGuid : _pcmSubtypeGuid,
			};
		}
		static T PtrToStruct<T>(IntPtr ptr) {
#if NET451_OR_GREATER || NETSTANDARD1_2_OR_GREATER || NETCOREAPP1_0_OR_GREATER
			return Marshal.PtrToStructure<T>(ptr);
#else
			return (T)Marshal.PtrToStructure(ptr, typeof(T));
#endif
		}
		public static WaveFormat FromInternalFormat(IntPtr ptr) {
			var format = PtrToStruct<WAVEFORMATEX>(ptr);
			return format.cbSize < 22 ? FromInternalFormat(format) : FromInternalFormat(PtrToStruct<WAVEFORMATEXTENSIBLE>(ptr));
		}
		public static WaveFormat FromInternalFormat(WAVEFORMATEXTENSIBLE value) {
			var format = value.Format;
			var result = FromInternalFormat(format);
			if (format.cbSize < 22)
				return result;
			int internalChannelMask = value.dwChannelMask;
			int channelMask = internalChannelMask & 0x3ffff;
			if (channelMask != internalChannelMask) throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The WASAPI channel mask {0} contains a channel that is not supported.", internalChannelMask));
			result.ChannelMask = (ChannelMask)channelMask;
			return result;
		}
		public static WaveFormat FromInternalFormat(WAVEFORMATEX value) => new() {
			Channels = value.nChannels,
			SampleRate = value.nSamplesPerSec,
			SampleFormat = FromInternalBitDepth(value.wBitsPerSample),
		};
		public static SampleFormat FromInternalBitDepth(ushort bitsPerSample) => bitsPerSample switch {
			8 => SampleFormat.U8,
			16 => SampleFormat.S16,
			24 => SampleFormat.S24,
			32 => SampleFormat.S32,
			_ => throw new NotSupportedException(),
		};
		public static AUDIO_STREAM_CATEGORY ToInternalStreamCategory(AudioUsage usage) => usage switch {
			AudioUsage.Media => AUDIO_STREAM_CATEGORY.Media,
			AudioUsage.Communication => AUDIO_STREAM_CATEGORY.Communications,
			AudioUsage.Alarm or
			AudioUsage.Notification or
			AudioUsage.NotificationRingtone or
			AudioUsage.NotificationEvent => AUDIO_STREAM_CATEGORY.SoundEffects,
			AudioUsage.AssistanceAccessibility or
			AudioUsage.AssistanceNavigation or
			AudioUsage.AssistanceSonification => AUDIO_STREAM_CATEGORY.Other,
			AudioUsage.Game => AUDIO_STREAM_CATEGORY.GameMedia,
			_ => AUDIO_STREAM_CATEGORY.Other,
		};
		public static int FromReferenceTime(uint sampleRate, long value) => (int)(value * sampleRate / 1e7 + 0.5);
		public static long ToReferenceTime(uint sampleRate, int value) => (long)(1e7 / sampleRate * value + 0.5);
	}
}
