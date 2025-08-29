using System;
using System.Globalization;
using System.Threading;

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
		/// Waits until the status mismatch with the given current status.
		/// </summary>
		/// <param name="currentStatus">The current status to avoid.</param>
		/// <param name="newStatus">The new status.</param>
		/// <param name="timeout">Timeout.</param>
		/// <returns>Whether the waiting operation completed within the given timeout.</returns>
		public virtual bool WaitForNextStatus(AudioClientStatus currentStatus, out AudioClientStatus newStatus, TimeSpan timeout) {
			TimeSpan sleepDuration = TimeSpan.FromMilliseconds(16);
			for (; Status == currentStatus; Thread.Sleep(sleepDuration)) {
				if (timeout <= TimeSpan.Zero) {
					newStatus = currentStatus;
					return false;
				}
				if (timeout < sleepDuration)
					sleepDuration = timeout;
				timeout -= sleepDuration;
			}
			newStatus = Status;
			return true;
		}

		readonly static TimeSpan _defaultTimeout = TimeSpan.FromSeconds(4);
		/// <summary>
		/// Starts the wave data transmission.
		/// </summary>
		public void Start() => Start(_defaultTimeout);
		/// <summary>
		/// Starts the wave data transmission.
		/// </summary>
		/// <param name="timeout">Timeout.</param>
		public void Start(TimeSpan timeout) {
			RequestStart();
			if (!WaitForNextStatus(AudioClientStatus.Starting, out var newStatus, timeout)) {
				throw new TimeoutException("Failed to start audio client: Timed out.");
			}
			if (newStatus != AudioClientStatus.Playing) {
				AudioStreamHelpers.ThrowStatusException(newStatus, "Failed to start audio client");
			}
		}
		/// <summary>
		/// Requests to start the wave data transmission.
		/// </summary>
		public abstract void RequestStart();
		/// <summary>
		/// Pauses the wave data transmission.
		/// </summary>
		/// <remarks>
		/// This method does not reset <see cref="Position" /> and <see cref="BufferPosition" />.
		/// </remarks>
		public void Pause() => Pause(_defaultTimeout);
		/// <summary>
		/// Pauses the wave data transmission.
		/// </summary>
		/// <param name="timeout">Timeout.</param>
		public void Pause(TimeSpan timeout) {
			RequestPause();
			if (!WaitForNextStatus(AudioClientStatus.Pausing, out var newStatus, timeout)) {
				throw new TimeoutException("Failed to pause audio client: Timed out.");
			}
			if (newStatus != AudioClientStatus.Idle) {
				AudioStreamHelpers.ThrowStatusException(newStatus, "Failed to pause audio client");
			}
		}
		/// <summary>
		/// Requests to pause the wave data transmission.
		/// </summary>
		public abstract void RequestPause();
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
