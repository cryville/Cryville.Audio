using Cryville.Common.Math;
using System;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioSource" /> that generates sound by a given function.
	/// </summary>
	public abstract class FunctionAudioSource : AudioSource {
		double _time;

		/// <summary>
		/// The channel count of the output format.
		/// </summary>
		protected int Channels => Format.Channels;

		/// <inheritdoc />
		protected sealed override void Dispose(bool disposing) { }

		/// <inheritdoc />
		public override bool EndOfData => false;

		/// <inheritdoc />
		protected internal sealed override bool IsFormatSupported(WaveFormat format) {
			return format.SampleFormat == SampleFormat.Unsigned8
				|| format.SampleFormat == SampleFormat.Signed16
				|| format.SampleFormat == SampleFormat.Signed24
				|| format.SampleFormat == SampleFormat.Signed32;
		}

		/// <inheritdoc />
		protected internal sealed override void FillBuffer(byte[] buffer, int offset, int length) {
			for (int i = offset; i < length + offset; _time += 1d / Format.SampleRate) {
				for (int j = 0; j < Format.Channels; j++) {
					float v = Func(_time, j);
					switch (Format.SampleFormat) {
						case SampleFormat.Unsigned8:
							buffer[i++] = ClampScale.ToByte(v);
							break;
						case SampleFormat.Signed16:
							short d16 = ClampScale.ToInt16(v);
							buffer[i++] = (byte)d16;
							buffer[i++] = (byte)(d16 >> 8);
							break;
						case SampleFormat.Signed24:
							int d24 = ClampScale.ToInt24(v);
							buffer[i++] = (byte)d24;
							buffer[i++] = (byte)(d24 >> 8);
							buffer[i++] = (byte)(d24 >> 16);
							break;
						case SampleFormat.Signed32:
							int d32 = ClampScale.ToInt32(v);
							buffer[i++] = (byte)d32;
							buffer[i++] = (byte)(d32 >> 8);
							buffer[i++] = (byte)(d32 >> 16);
							buffer[i++] = (byte)(d32 >> 24);
							break;
					}
				}
			}
		}

		/// <summary>
		/// The function used to generate wave.
		/// </summary>
		/// <param name="time">The time position.</param>
		/// <param name="channel">The channel index.</param>
		protected abstract float Func(double time, int channel);
	}
}
