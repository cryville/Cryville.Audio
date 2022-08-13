using System;
using System.Globalization;

namespace Cryville.Audio {
	/// <summary>
	/// The wave format.
	/// </summary>
	public struct WaveFormat {
		/// <summary>
		/// The channel count.
		/// </summary>
		public ushort Channels { get; set; }
		/// <summary>
		/// The sample rate (samples per second.)
		/// </summary>
		public uint SampleRate { get; set; }
		/// <summary>
		/// Bit count per sample.
		/// </summary>
		public ushort BitsPerSample { get; set; }

		/// <summary>
		/// Bytes per second.
		/// </summary>
		public uint BytesPerSecond => Channels * SampleRate * BitsPerSample / 8;

		/// <summary>
		/// The default wave format.
		/// </summary>
		public readonly static WaveFormat Default = new WaveFormat {
			Channels = 2, SampleRate = 48000, BitsPerSample = 16
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
	}
}