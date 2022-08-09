using System;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioSource" /> that generates single tone.
	/// </summary>
	public sealed class SingleToneAudioSource : FunctionAudioSource {
		/// <summary>
		/// The tone type.
		/// </summary>
		public ToneType Type { get; set; }
		/// <summary>
		/// The frequency of the wave.
		/// </summary>
		public float Frequency { get; set; }
		/// <summary>
		/// The amplitude of the wave.
		/// </summary>
		public float Amplitude { get; set; }
		protected override float Func(double time, int channel) {
			float v = Amplitude;
			switch (Type) {
				case ToneType.Sine: v *= (float)Math.Sin(time * Frequency * 2 * Math.PI); break;
				case ToneType.Triangle: v *= (float)(time * Frequency % 1 - 0.5); break;
				case ToneType.Square: if (time * Frequency % 1 >= 0.5) v *= -1; break;
			}
			return v;
		}
	}
	/// <summary>
	/// Tone type.
	/// </summary>
	public enum ToneType {
		/// <summary>
		/// Sine wave.
		/// </summary>
		Sine,
		/// <summary>
		/// Triangle wave.
		/// </summary>
		Triangle,
		/// <summary>
		/// Square wave.
		/// </summary>
		Square,
	}
}
