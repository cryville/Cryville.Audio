using System;
using System.Globalization;

namespace Cryville.Audio {
	/// <summary>
	/// The wave format.
	/// </summary>
	public struct WaveFormat : IEquatable<WaveFormat> {
		/// <summary>
		/// The channel count.
		/// </summary>
		public ushort Channels { get; set; }
		/// <summary>
		/// The sample rate (samples per second.)
		/// </summary>
		public uint SampleRate { get; set; }
		/// <summary>
		/// The sample format.
		/// </summary>
		public SampleFormat SampleFormat { get; set; }
		/// <summary>
		/// Bit count per sample.
		/// </summary>
		public ushort BitsPerSample {
			get {
				switch (SampleFormat) {
					case SampleFormat.Unsigned8: return 8;
					case SampleFormat.Signed16: return 16;
					case SampleFormat.Signed24: return 24;
					case SampleFormat.Signed32:
					case SampleFormat.Binary32: return 32;
					case SampleFormat.Binary64: return 64;
					default: throw new InvalidOperationException(); // Unreachable
				}
			}
		}

		/// <summary>
		/// Bytes per second.
		/// </summary>
		public uint BytesPerSecond => Channels * SampleRate * BitsPerSample / 8;

		/// <summary>
		/// The default wave format.
		/// </summary>
		public readonly static WaveFormat Default = new WaveFormat {
			Channels = 2, SampleRate = 48000, SampleFormat = SampleFormat.Signed16
		};

		/// <summary>
		/// Gets the aligned buffer size.
		/// </summary>
		/// <param name="size">The prefered buffer size in bytes.</param>
		/// <returns>The aligned buffer size in bytes.</returns>
		public int Align(double size) {
			int block = Channels * BitsPerSample / 8;
			return (int)Math.Ceiling(size / block) * block;
		}

		/// <inheritdoc />
		public override string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}ch * {1}Hz * {2}bits", Channels, SampleRate, BitsPerSample);
		}

		/// <inheritdoc />
		public bool Equals(WaveFormat other) {
			if (Channels != other.Channels) return false;
			if (SampleRate != other.SampleRate) return false;
			if (SampleFormat != other.SampleFormat) return false;
			return true;
		}

		/// <inheritdoc />
		public override bool Equals(object obj) {
			if (obj is WaveFormat other)
				return Equals(other);
			return false;
		}

		/// <inheritdoc />
		public override int GetHashCode() {
			return Channels ^ (int)SampleRate ^ ((int)SampleFormat << 16);
		}

		/// <inheritdoc />
		public static bool operator ==(WaveFormat left, WaveFormat right) {
			return left.Equals(right);
		}

		/// <inheritdoc />
		public static bool operator !=(WaveFormat left, WaveFormat right) {
			return !(left == right);
		}
	}
	/// <summary>
	/// Sample format.
	/// </summary>
	public enum SampleFormat {
		/// <summary>
		/// Unsigned 8-bit integer sample format.
		/// </summary>
		Unsigned8,
		/// <summary>
		/// Signed 16-bit integer sample format.
		/// </summary>
		Signed16,
		/// <summary>
		/// Signed 24-bit integer sample format.
		/// </summary>
		Signed24,
		/// <summary>
		/// Signed 32-bit integer sample format.
		/// </summary>
		Signed32,
		/// <summary>
		/// IEEE 754 single precision floating-point sample format.
		/// </summary>
		Binary32,
		/// <summary>
		/// IEEE 754 double precision floating-point sample format.
		/// </summary>
		Binary64,
	}
}