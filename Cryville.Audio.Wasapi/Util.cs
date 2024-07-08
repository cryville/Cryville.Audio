using Microsoft.Windows.MMDevice;
using Microsoft.Windows.Mme;
using System;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.Wasapi {
	internal static class Util {
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
		public static WAVEFORMATEX ToInternalFormat(WaveFormat value) {
			ushort blockAlign = (ushort)value.FrameSize;
			return new WAVEFORMATEX {
				wFormatTag = (ushort)WAVE_FORMAT.PCM,
				nChannels = value.Channels,
				nSamplesPerSec = value.SampleRate,
				nAvgBytesPerSec = value.SampleRate * blockAlign,
				nBlockAlign = blockAlign,
				wBitsPerSample = value.BitsPerSample,
				cbSize = 0,
			};
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
		public static int FromReferenceTime(uint sampleRate, long value) => (int)(value * sampleRate / 1e7 + 0.5);
		public static long ToReferenceTime(uint sampleRate, int value) => (long)(1e7 / sampleRate * value + 0.5);
	}
}
