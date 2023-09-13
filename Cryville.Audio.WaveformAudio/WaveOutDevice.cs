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
		public float DefaultBufferDuration => 40;

		/// <inheritdoc />
		public float MinimumBufferDuration => 0;

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
		public bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			ushort ch = format.Channels;
			byte flagch;
			switch (ch) {
				case 1: flagch = 0; break;
				case 2: flagch = 1; break;
				default:
					format.Channels = 2;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			uint sr = format.SampleRate;
			byte flagsr;
			switch (sr) {
				case 11025: flagsr = 0; break;
				case 22050: flagsr = 1; break;
				case 44100: flagsr = 2; break;
				case 48000: flagsr = 3; break;
				case 96000: flagsr = 4; break;
				default:
					if (sr <= 11025) format.SampleRate = 11025;
					else if (sr <= 22050) format.SampleRate = 22050;
					else if (sr <= 44100) format.SampleRate = 44100;
					else if (sr <= 48000) format.SampleRate = 48000;
					else format.SampleRate = 96000;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			SampleFormat bits = format.SampleFormat;
			byte flagbits;
			switch (bits) {
				case SampleFormat.U8: flagbits = 0; break;
				case SampleFormat.S16: flagbits = 1; break;
				default:
					format.SampleFormat = SampleFormat.S16;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			var iwf = 1 << (flagch | (flagbits << 1) | (flagsr << 2));
			uint capfilter = _caps.dwFormats;
			if ((capfilter & iwf) != 0) {
				suggestion = format;
				return true;
			}
			else {
				uint chfilter = 0x55555555U;
				if (flagch == 1) chfilter = ~chfilter;
				if ((capfilter & chfilter) != 0) capfilter &= chfilter;
				else capfilter &= ~chfilter;
				if (capfilter == 0) {
					suggestion = null;
					return false;
				}
				bool srmatchflag = false;
				for (byte iflagsr = flagsr; iflagsr < 8; iflagsr++) {
					uint srfilter = 0x0000000fU << (iflagsr << 2);
					if ((capfilter & srfilter) != 0) {
						capfilter &= srfilter;
						srmatchflag = true;
						break;
					}
				}
				if (!srmatchflag && flagsr > 0)
					for (byte iflagsr = (byte)(flagsr - 1); iflagsr >= 0; iflagsr--) {
						uint srfilter = 0x0000000fU << (iflagsr << 2);
						if ((capfilter & srfilter) != 0) {
							capfilter &= srfilter;
							srmatchflag = true;
							break;
						}
					}
				if (!srmatchflag) {
					suggestion = null;
					return false;
				}
				uint bitsfilter = 0x33333333U;
				if (flagbits == 1) bitsfilter = ~bitsfilter;
				if ((capfilter & flagbits) != 0) capfilter &= bitsfilter;
				else capfilter &= ~bitsfilter;
				if (capfilter == 0) {
					suggestion = null;
					return false;
				}
				uint sugsr;
				if ((capfilter & 0x0000000f) != 0) sugsr = 11025;
				else if ((capfilter & 0x000000f0) != 0) sugsr = 22050;
				else if ((capfilter & 0x00000f00) != 0) sugsr = 44100;
				else if ((capfilter & 0x0000f000) != 0) sugsr = 48000;
				else if ((capfilter & 0x000f0000) != 0) sugsr = 96000;
				else throw new NotSupportedException("Theoretically unreachable");
				suggestion = new WaveFormat {
					Channels = (capfilter & 0x55555555) != 0 ? (ushort)1 : (ushort)2,
					SampleRate = sugsr,
					SampleFormat = (capfilter & 0x33333333) != 0 ? SampleFormat.U8 : SampleFormat.S16,
				};
				return false;
			}
		}

		/// <inheritdoc />
		public AudioClient Connect(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			return new WaveOutClient(this, format, bufferDuration, shareMode);
		}
	}
}
