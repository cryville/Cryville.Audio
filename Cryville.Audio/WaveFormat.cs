using System;
using System.Globalization;

namespace Cryville.Audio {
	/// <summary>
	/// The wave format.
	/// </summary>
	/// <remarks>
	/// <para><see cref="ChannelMask" /> should be set explicitly if there are more than two channels.</para>
	/// </remarks>
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
		/// The channel layout.
		/// </summary>
		public ChannelMask ChannelMask { get; set; }
		/// <summary>
		/// Determines whether the number of bits set in <see cref="ChannelMask" /> equals to <see cref="Channels" />.
		/// </summary>
		/// <returns>Whether the number of bits set in <see cref="ChannelMask" /> equals to <see cref="Channels" />.</returns>
		public readonly bool IsChannelMaskValid() {
			int mask = (int)ChannelMask;
			int count = 0;
			while (mask != 0) {
				count++;
				mask &= mask - 1;
			}
			return count == Channels;
		}
		/// <summary>
		/// Assigns the default channel mask for the given channel count.
		/// </summary>
		/// <returns>Whether a default channel mask is found.</returns>
		public bool AssignDefaultChannelMask() {
			ChannelMask = Channels switch {
				1 => ChannelMask.Mono,
				2 => ChannelMask.Stereo,
				3 => ChannelMask.Tri,
				4 => ChannelMask.Quad,
				5 => ChannelMask.FiveBack,
				6 => ChannelMask.FiveBack | ChannelMask.LFPoint1,
				7 => ChannelMask.SixBack | ChannelMask.LFPoint1,
				8 => ChannelMask.Seven | ChannelMask.LFPoint1,
				_ => 0,
			};
			return ChannelMask != 0;
		}
		/// <summary>
		/// Validates <see cref="ChannelMask" />, assigning the default channel mask if it is not specified.
		/// </summary>
		/// <exception cref="InvalidOperationException">No default channel mask is found. -or- The set channel mask mismatches with the channel count.</exception>
		public void ValidateChannelMask() {
			if (ChannelMask == 0 && !AssignDefaultChannelMask())
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Mo default channel mask defined for {0} channels.", Channels));
			if (!IsChannelMaskValid())
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Channel mask {0} mismatched with channel count {1}.", ChannelMask, Channels));
		}

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
			Channels = 2,
			SampleRate = 48000,
			SampleFormat = SampleFormat.S16,
			ChannelMask = ChannelMask.Stereo
		};

		/// <inheritdoc />
		public override readonly string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}ch ({3}) * {1}Hz * {2}bits", Channels, SampleRate, BitsPerSample, ChannelMask);
		}

		/// <inheritdoc />
		public readonly bool Equals(WaveFormat other) {
			if (Channels != other.Channels) return false;
			if (SampleRate != other.SampleRate) return false;
			if (SampleFormat != other.SampleFormat) return false;
			if (ChannelMask != other.ChannelMask) return false;
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
			return Channels ^ (int)SampleRate ^ ((int)SampleFormat << 16) ^ (int)ChannelMask;
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
