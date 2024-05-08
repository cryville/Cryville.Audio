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
		/// The sample rate (samples per channel per second, i.e. frames per second.)
		/// </summary>
		public uint SampleRate { get; set; }
		/// <summary>
		/// The sample format.
		/// </summary>
		public SampleFormat SampleFormat { get; set; }
		/// <summary>
		/// Bit count per sample.
		/// </summary>
		public readonly ushort BitsPerSample => SampleFormat switch {
			SampleFormat.U8 => 8,
			SampleFormat.S16 => 16,
			SampleFormat.S24 => 24,
			SampleFormat.S32 or SampleFormat.F32 => 32,
			SampleFormat.F64 => 64,
			_ => throw new InvalidOperationException(), // Unreachable
		};

		/// <summary>
		/// Bytes per frame.
		/// </summary>
		public readonly int FrameSize => Channels * BitsPerSample / 8;

		/// <summary>
		/// Bytes per second.
		/// </summary>
		public readonly uint BytesPerSecond => SampleRate * (uint)FrameSize;

		/// <summary>
		/// The default wave format.
		/// </summary>
		public static readonly WaveFormat Default = new() {
			Channels = 2, SampleRate = 48000, SampleFormat = SampleFormat.S16
		};

		/// <summary>
		/// Gets the aligned buffer size.
		/// </summary>
		/// <param name="size">The prefered buffer size in bytes.</param>
		/// <param name="floored">Whether the result is floored or ceiled.</param>
		/// <returns>The aligned buffer size in bytes.</returns>
		public readonly long Align(long size, bool floored = false) {
			if (size % FrameSize == 0) return size;
			size /= FrameSize;
			if (!floored) size++;
			return size * FrameSize;
		}

		/// <summary>
		/// Gets the aligned buffer size.
		/// </summary>
		/// <param name="size">The prefered buffer size in bytes.</param>
		/// <param name="floored">Whether the result is floored or ceiled.</param>
		/// <returns>The aligned buffer size in bytes.</returns>
		public readonly long Align(double size, bool floored = false) {
			if (size < 0 || size > long.MaxValue) throw new ArgumentOutOfRangeException(nameof(size));
			size /= FrameSize;
			long blockNum = (long)(floored ? Math.Floor(size) : Math.Ceiling(size));
			return blockNum * FrameSize;
		}

		/// <inheritdoc />
		public override readonly string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}ch * {1}Hz * {2}bits", Channels, SampleRate, BitsPerSample);
		}

		/// <inheritdoc />
		public readonly bool Equals(WaveFormat other) {
			if (Channels != other.Channels) return false;
			if (SampleRate != other.SampleRate) return false;
			if (SampleFormat != other.SampleFormat) return false;
			return true;
		}

		/// <inheritdoc />
		public override readonly bool Equals(object obj) {
			if (obj is WaveFormat other)
				return Equals(other);
			return false;
		}

		/// <inheritdoc />
		public override readonly int GetHashCode() {
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
		/// Invalid sample format.
		/// </summary>
		Invalid = 0b0,
		/// <summary>
		/// Unsigned 8-bit integer sample format.
		/// </summary>
		U8 = 0b10000,
		/// <summary>
		/// Signed 16-bit integer sample format.
		/// </summary>
		S16 = 0b0010,
		/// <summary>
		/// Signed 24-bit integer sample format.
		/// </summary>
		S24 = 0b0011,
		/// <summary>
		/// Signed 32-bit integer sample format.
		/// </summary>
		S32 = 0b0100,
		/// <summary>
		/// IEEE 754 single precision floating-point sample format.
		/// </summary>
		F32 = 0b100100,
		/// <summary>
		/// IEEE 754 double precision floating-point sample format.
		/// </summary>
		F64 = 0b100110,
	}
}
