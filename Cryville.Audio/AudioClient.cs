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
		/// The current wave format of the connection.
		/// </summary>
		public abstract WaveFormat Format { get; }
		/// <summary>
		/// The buffer size in frames.
		/// </summary>
		public abstract int BufferSize { get; }
		/// <summary>
		/// The maximum latency of the connection in milliseconds.
		/// </summary>
		/// <remarks>
		/// <para>May be zero if the API does not provide this value.</para>
		/// </remarks>
		public abstract float MaximumLatency { get; }
		/// <summary>
		/// Whether the client is playing.
		/// </summary>
		public bool Playing { get; private set; }
		/// <summary>
		/// Whether the client is muted.
		/// </summary>
		public bool Muted { get; set; }
		/// <summary>
		/// The current position of the device stream in seconds.
		/// </summary>
		public abstract double Position { get; }
		/// <summary>
		/// The current position of the buffer in seconds.
		/// </summary>
		public abstract double BufferPosition { get; }

		private AudioStream m_source;
		/// <summary>
		/// The audio source.
		/// </summary>
		public AudioStream Source {
			get => m_source;
			set {
				if (Device.DataFlow != DataFlow.Out)
					throw new NotSupportedException("Wrong data-flow direction.");
				value?.SetFormat(Format, BufferSize);
				m_source = value;
			}
		}

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
