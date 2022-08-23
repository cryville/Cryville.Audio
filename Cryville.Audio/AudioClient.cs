using Cryville.Audio.Source;
using System;

namespace Cryville.Audio {
	/// <summary>
	/// Audio client that manages connection to a <see cref="IAudioDevice" />.
	/// </summary>
	public abstract class AudioClient : IDisposable {
		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected abstract void Dispose(bool disposing);

		/// <summary>
		/// The device of the client.
		/// </summary>
		public abstract IAudioDevice Device { get; }
		/// <summary>
		/// The default buffer duration of the client.
		/// </summary>
		public abstract float DefaultBufferDuration { get; }
		/// <summary>
		/// The minimum buffer duration of the client.
		/// </summary>
		public abstract float MinimumBufferDuration { get; }
		/// <summary>
		/// The default wave format of the device.
		/// </summary>
		public abstract WaveFormat DefaultFormat { get; }
		/// <summary>
		/// The current wave format of the connection.
		/// </summary>
		public abstract WaveFormat Format { get; }
		/// <summary>
		/// The buffer size in bytes.
		/// </summary>
		public abstract int BufferSize { get; }
		/// <summary>
		/// The maximum latency of the connection in milliseconds.
		/// </summary>
		public abstract float MaximumLatency { get; }
		/// <summary>
		/// Whether the client is playing.
		/// </summary>
		public bool Playing { get; private set; }
		/// <summary>
		/// The current position of the device stream in seconds.
		/// </summary>
		public abstract double Position { get; }
		/// <summary>
		/// The current position of the buffer in seconds.
		/// </summary>
		public abstract double BufferPosition { get; }

		private AudioSource m_source;
		/// <summary>
		/// The audio source.
		/// </summary>
		public AudioSource Source {
			get => m_source;
			set {
				if (Device.DataFlow != DataFlow.Out)
					throw new NotSupportedException("Wrong data-flow direction");
				if (m_source != null)
					m_source.SetFormat(WaveFormat.Default, 0);
				m_source = value;
				if (m_source != null)
					m_source.SetFormat(Format, BufferSize);
			}
		}

		/// <summary>
		/// Gets whether <paramref name="format" /> is supported by the device.
		/// </summary>
		/// <param name="format">The specified wave format.</param>
		/// <param name="suggestion">A wave format suggested by the device. <paramref name="format" /> if it is supported. <see langword="null" /> if no format is supported.</param>
		/// <param name="shareMode">The share mode.</param>
		/// <returns>Whether <paramref name="format" /> is supported.</returns>
		public abstract bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared);
		/// <summary>
		/// Initialize the client.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="bufferDuration">The buffer duration of the connection in milliseconds.</param>
		/// <param name="shareMode">The share mode of the connection.</param>
		/// <remarks>Different operations may occur with different API being used. Please also see the documentations of the implementing classes.</remarks>
		public abstract void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared);
		/// <summary>
		/// Starts the wave data transmission.
		/// </summary>
		/// <remarks>
		/// If <see cref="Source" /> is <see langword="null" /> while playing, the output will be silence.
		/// </remarks>
		public virtual void Start() { Playing = true; }
		/// <summary>
		/// Pauses the wave data transmission.
		/// </summary>
		/// <remarks>
		/// This method does not reset <see cref="Position" /> and <see cref="BufferPosition" />.
		/// </remarks>
		public virtual void Pause() { Playing = false; }
	}
}
