using System;

namespace Cryville.Audio {
	/// <summary>
	/// Represents a builder that builds <see cref="AudioStream" />.
	/// </summary>
	public interface IAudioStreamBuilder {
		/// <summary>
		/// The default wave format of the stream.
		/// </summary>
		WaveFormat DefaultFormat { get; }
		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the audio stream.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <remarks>
		/// <para>When implemented, must return <see langword="true" /> when <paramref name="format" /> equals to <see cref="DefaultFormat" />.</para>
		/// </remarks>
		bool IsFormatSupported(WaveFormat format);
		/// <summary>
		/// Builds the audio stream.
		/// </summary>
		/// <param name="format">The wave format of the audio stream.</param>
		/// <returns>The built audio stream.</returns>
		/// <exception cref="NotSupportedException"><paramref name="format" /> is not supported.</exception>
		AudioStream Build(WaveFormat format);
	}
	/// <summary>
	/// Represents a builder that builds a specific type of <see cref="AudioStream" />.
	/// </summary>
	/// <typeparam name="T">The type of the audio stream.</typeparam>
	public interface IAudioStreamBuilder<out T> : IAudioStreamBuilder where T : AudioStream {
		/// <summary>
		/// Builds the audio stream.
		/// </summary>
		/// <param name="format">The wave format of the audio stream.</param>
		/// <returns>The built audio stream.</returns>
		/// <exception cref="NotSupportedException"><paramref name="format" /> is not supported.</exception>
		new T Build(WaveFormat format);
	}
	/// <summary>
	/// A builder that builds a specific type of <see cref="AudioStream" />.
	/// </summary>
	/// <typeparam name="T">The type of the audio stream.</typeparam>
	public abstract class AudioStreamBuilder<T> : IAudioStreamBuilder<T> where T : AudioStream {
		/// <inheritdoc />
		public abstract WaveFormat DefaultFormat { get; }
		/// <inheritdoc />
		public virtual bool IsFormatSupported(WaveFormat format) => format == DefaultFormat;

		AudioStream IAudioStreamBuilder.Build(WaveFormat format) => Build(format);
		/// <inheritdoc />
		public abstract T Build(WaveFormat format);
	}
}
