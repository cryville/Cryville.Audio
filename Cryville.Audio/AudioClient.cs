using System;
using System.Globalization;

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
		protected virtual void Dispose(bool disposing) {
			if (disposing) Close();
		}

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
		/// The status of the connection.
		/// </summary>
		public abstract AudioClientStatus Status { get; }
		/// <summary>
		/// The current position of the device stream in seconds.
		/// </summary>
		public abstract double Position { get; }
		/// <summary>
		/// The current position of the buffer in seconds.
		/// </summary>
		public abstract double BufferPosition { get; }

		AudioStream? m_stream;
		/// <summary>
		/// The audio stream.
		/// </summary>
		/// <remarks>
		/// If the value of this property is <see langword="null" /> while playing, the output will be silence.
		/// </remarks>
		public AudioStream? Stream {
			get => m_stream;
			set {
				if (value != null) {
					var format = Format;
					format.ValidateChannelMask();
					if (value.Format != format)
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Mismatched audio format. Client: {0}. Source: {1}.", format, value.Format));
					value.BufferSize = BufferSize;
				}
				m_stream = value;
				OnSetStream();
			}
		}
		/// <summary>
		/// Called when the source is set.
		/// </summary>
		protected virtual void OnSetStream() { }

		/// <summary>
		/// Starts the wave data transmission.
		/// </summary>
		/// <remarks>
		/// If <see cref="Source" /> is <see langword="null" /> while playing, the output will be silence.
		/// </remarks>
		public abstract void Start();
		/// <summary>
		/// Pauses the wave data transmission.
		/// </summary>
		/// <remarks>
		/// This method does not reset <see cref="Position" /> and <see cref="BufferPosition" />.
		/// </remarks>
		public abstract void Pause();
		/// <summary>
		/// Closes the connection.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Raised when the audio client is disconnected during playback.
		/// </summary>
		public event EventHandler? PlaybackDisconnected;
		/// <summary>
		/// Called by a derived class when the audio client is disconnected during playback.
		/// </summary>
		protected void OnPlaybackDisconnected() {
			PlaybackDisconnected?.Invoke(this, EventArgs.Empty);
		}
	}
}
