using Microsoft.Windows.Mme;
using System;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.WaveformAudio {
	internal static class Util {
		public static WAVEFORMATEX ToInternalFormat(WaveFormat value) {
			ushort blockAlign = (ushort)(value.Channels * value.BitsPerSample / 8);
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
	}
}
