using Microsoft.Windows.MMDevice;
using Microsoft.Windows.Mme;
using System;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.Wasapi {
	internal static class Util {
		public static DataFlow FromInternalDataFlowEnum(EDataFlow value) {
			switch (value) {
				case EDataFlow.eRender: return DataFlow.Out;
				case EDataFlow.eCapture: return DataFlow.In;
				case EDataFlow.eAll: return DataFlow.All;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
		public static EDataFlow ToInternalDataFlowEnum(DataFlow value) {
			switch (value) {
				case DataFlow.Out: return EDataFlow.eRender;
				case DataFlow.In: return EDataFlow.eCapture;
				case DataFlow.All: return EDataFlow.eAll;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
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
		public static WaveFormat FromInternalFormat(WAVEFORMATEX value) {
			return new WaveFormat {
				Channels = value.nChannels,
				SampleRate = value.nSamplesPerSec,
				SampleFormat = FromInternalBitDepth(value.wBitsPerSample),
			};
		}
		public static SampleFormat FromInternalBitDepth(ushort bitsPerSample) {
			switch (bitsPerSample) {
				case 8: return SampleFormat.U8;
				case 16: return SampleFormat.S16;
				case 24: return SampleFormat.S24;
				case 32: return SampleFormat.S32;
				default: throw new NotSupportedException();
			}
		}
		public static int FromReferenceTime(uint sampleRate, long value) {
			return (int)(value * sampleRate / 1e7 + 0.5);
		}
		public static long ToReferenceTime(uint sampleRate, int value) {
			return (long)(1e7 / sampleRate * value + 0.5);
		}
	}
}
