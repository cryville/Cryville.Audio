using Microsoft.Windows.Mme;
using Microsoft.Windows.MmSysCom;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.WaveformAudio {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with WinMM.
	/// </summary>
	public class WaveOutDevice : IAudioDevice {
		internal WaveOutDevice(uint index) {
			Index = index;
			MmSysComExports.MMR(MmeExports.waveOutGetDevCapsW(index, out _caps, (uint)Marshal.SizeOf(_caps)));
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected virtual void Dispose(bool disposing) { }

		internal readonly uint Index;
		internal readonly WAVEOUTCAPSW _caps;

		/// <summary>
		/// The friendly name of the device.
		/// </summary>
		/// <remarks>Due to technical reason, this field is truncated if it has more than 31 characters.</remarks>
		public string Name => _caps.szPname;

		/// <inheritdoc />
		public DataFlow DataFlow => DataFlow.Out;

		/// <inheritdoc />
		public int BurstSize => 0;
		/// <inheritdoc />
		public int MinimumBufferSize => 0;
		/// <inheritdoc />
		public int DefaultBufferSize => 1920;

		/// <inheritdoc />
		public WaveFormat DefaultFormat {
			get {
				IsFormatSupported(
					new WaveFormat { Channels = 2, SampleRate = 192000, SampleFormat = SampleFormat.S32 },
					out var format
				);
				return format ?? throw new NotSupportedException("No format is supported.");
			}
		}

		/// <inheritdoc />
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			ushort ch = format.Channels;
			byte flagCH;
			switch (ch) {
				case 1: flagCH = 0; break;
				case 2: flagCH = 1; break;
				default:
					format.Channels = 2;
					IsFormatSupported(format, out suggestion, usage, shareMode);
					return false;
			}
			uint sr = format.SampleRate;
			byte flagSR;
			switch (sr) {
				case 11025: flagSR = 0; break;
				case 22050: flagSR = 1; break;
				case 44100: flagSR = 2; break;
				case 48000: flagSR = 3; break;
				case 96000: flagSR = 4; break;
				default:
					if (sr <= 11025) format.SampleRate = 11025;
					else if (sr <= 22050) format.SampleRate = 22050;
					else if (sr <= 44100) format.SampleRate = 44100;
					else if (sr <= 48000) format.SampleRate = 48000;
					else format.SampleRate = 96000;
					IsFormatSupported(format, out suggestion, usage, shareMode);
					return false;
			}
			SampleFormat bits = format.SampleFormat;
			byte flagBits;
			switch (bits) {
				case SampleFormat.U8: flagBits = 0; break;
				case SampleFormat.S16: flagBits = 1; break;
				default:
					format.SampleFormat = SampleFormat.S16;
					IsFormatSupported(format, out suggestion, usage, shareMode);
					return false;
			}
			var iwf = 1 << (flagCH | (flagBits << 1) | (flagSR << 2));
			uint capFilter = _caps.dwFormats;
			if ((capFilter & iwf) != 0) {
				suggestion = format;
				return true;
			}
			else {
				uint chFilter = 0x55555555U;
				if (flagCH == 1) chFilter = ~chFilter;
				if ((capFilter & chFilter) != 0) capFilter &= chFilter;
				else capFilter &= ~chFilter;
				if (capFilter == 0) {
					suggestion = null;
					return false;
				}
				bool srMatchFlag = false;
				for (byte iFlagSR = flagSR; iFlagSR < 8; iFlagSR++) {
					uint srFilter = 0x0000000fU << (iFlagSR << 2);
					if ((capFilter & srFilter) != 0) {
						capFilter &= srFilter;
						srMatchFlag = true;
						break;
					}
				}
				if (!srMatchFlag && flagSR > 0)
					for (byte iFlagSR = (byte)(flagSR - 1); iFlagSR >= 0; iFlagSR--) {
						uint srFilter = 0x0000000fU << (iFlagSR << 2);
						if ((capFilter & srFilter) != 0) {
							capFilter &= srFilter;
							srMatchFlag = true;
							break;
						}
					}
				if (!srMatchFlag) {
					suggestion = null;
					return false;
				}
				uint bitsFilter = 0x33333333U;
				if (flagBits == 1) bitsFilter = ~bitsFilter;
				if ((capFilter & flagBits) != 0) capFilter &= bitsFilter;
				else capFilter &= ~bitsFilter;
				if (capFilter == 0) {
					suggestion = null;
					return false;
				}
				uint sugSR;
				if ((capFilter & 0x0000000f) != 0) sugSR = 11025;
				else if ((capFilter & 0x000000f0) != 0) sugSR = 22050;
				else if ((capFilter & 0x00000f00) != 0) sugSR = 44100;
				else if ((capFilter & 0x0000f000) != 0) sugSR = 48000;
				else if ((capFilter & 0x000f0000) != 0) sugSR = 96000;
				else throw new NotSupportedException("Theoretically unreachable");
				suggestion = new WaveFormat {
					Channels = (capFilter & 0x55555555) != 0 ? (ushort)1 : (ushort)2,
					SampleRate = sugSR,
					SampleFormat = (capFilter & 0x33333333) != 0 ? SampleFormat.U8 : SampleFormat.S16,
				};
				return false;
			}
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, int bufferSize = 0, AudioUsage usage = AudioUsage.Media, AudioShareMode shareMode = AudioShareMode.Shared) {
			return new WaveOutClient(this, format, bufferSize, shareMode);
		}
	}
}
