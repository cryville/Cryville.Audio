using Microsoft.Windows.Mme;
using WAVE_FORMAT = Microsoft.Windows.MmReg.WAVE_FORMAT;

namespace Cryville.Audio.WinMM {
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
				BitsPerSample = value.wBitsPerSample,
			};
		}
	}
}
