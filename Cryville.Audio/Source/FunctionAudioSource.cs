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

		protected sealed override void Dispose(bool disposing) { }

		public override bool EndOfData => false;

		protected internal sealed override bool IsFormatSupported(WaveFormat format) {
			return format.BitsPerSample == 8
				|| format.BitsPerSample == 16
				|| format.BitsPerSample == 32;
		}

		protected internal sealed override void FillBuffer(byte[] buffer, int offset, int length) {
			for (int i = offset; i < length + offset; _time += 1d / Format.SampleRate) {
				for (int j = 0; j < Format.Channels; j++) {
					float v = Func(_time, j);
					switch (Format.BitsPerSample) {
						case 8:
							buffer[i++] = C8(v);
							break;
						case 16:
							short d16 = C16(v);
							buffer[i++] = (byte)d16;
							buffer[i++] = (byte)(d16 >> 8);
							break;
						case 32:
							int d32 = C32(v);
							buffer[i++] = (byte)d32;
							buffer[i++] = (byte)(d32 >> 8);
							buffer[i++] = (byte)(d32 >> 16);
							buffer[i++] = (byte)(d32 >> 24);
							break;
					}
				}
			}
		}
		static byte C8(float v) {
			v = v * 0x80 + 0x80;
			if (v >= byte.MaxValue) return byte.MaxValue;
			if (v < byte.MinValue) return byte.MinValue;
			return (byte)v;
		}
		static short C16(float v) {
			v *= 0x8000;
			if (v >= short.MaxValue) return short.MaxValue;
			if (v < short.MinValue) return short.MinValue;
			return (short)v;
		}
		static int C32(float v) {
			v *= 0x80000000;
			if (v >= int.MaxValue) return int.MaxValue;
			if (v < int.MinValue) return int.MinValue;
			return (int)v;
		}

		/// <summary>
		/// The function used to generate wave.
		/// </summary>
		/// <param name="time">The time position.</param>
		/// <param name="channel">The channel index.</param>
		protected abstract float Func(double time, int channel);
	}
}
