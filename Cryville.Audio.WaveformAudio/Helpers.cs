using Microsoft.Windows.Mme;
using System;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.WaveformAudio {
	internal static class Helpers {
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
		public static SampleFormat FromInternalBitDepth(ushort bitsPerSample) => bitsPerSample switch {
			8 => SampleFormat.U8,
			16 => SampleFormat.S16,
			24 => SampleFormat.S24,
			32 => SampleFormat.S32,
			_ => throw new NotSupportedException(),
		};
	}
}
