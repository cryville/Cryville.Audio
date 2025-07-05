using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Cryville.Audio {
	/// <summary>
	/// A context as an <see cref="AudioClient" /> proxy that reconnects the audio device automatically on disconnection.
	/// </summary>
	/// <param name="manager">The audio device manager.</param>
	/// <param name="dataFlow">The data-flow direction.</param>
	/// <remarks>
	/// <para>Call <see cref="Init" /> to initialize the context.</para>
	/// </remarks>
	public abstract class AutoReconnectContext(IAudioDeviceManager manager, DataFlow dataFlow) : AudioClient {
		[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
		[SuppressMessage("Usage", "CA2213", Justification = "Not owned")]
		readonly IAudioDeviceManager _manager = manager ?? throw new ArgumentNullException(nameof(manager));

		/// <inheritdoc />
		public override IAudioDevice Device => _client?.Device ?? throw new InvalidOperationException("Not initialized.");

		WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format => _client != null ? m_format : throw new InvalidOperationException("Not initialized.");

		int m_bufferSize;
		/// <inheritdoc />
		public override int BufferSize => _client != null ? m_bufferSize : throw new InvalidOperationException("Not initialized.");

		/// <inheritdoc />
		public override float MaximumLatency => _client?.MaximumLatency ?? throw new InvalidOperationException("Not initialized.");

		readonly object _statusLock = new();
		volatile AudioClientStatus m_status;
		/// <inheritdoc />
		public override AudioClientStatus Status => m_status;

		/// <inheritdoc />
		public override double Position {
			get {
				try {
					return _client?.Position ?? throw new InvalidOperationException("Not initialized.");
				}
				catch (AudioClientDisconnectedException) {
					OnAudioClientDisconnected();
					return Position;
				}
			}
		}

		/// <inheritdoc />
		public override double BufferPosition => _client?.BufferPosition ?? throw new InvalidOperationException("Not initialized.");

		/// <inheritdoc />
		public override void Start() {
			if (_client == null) throw new InvalidOperationException("Not initialized.");
			lock (_statusLock) {
				switch (m_status) {
					case AudioClientStatus.Playing:
					case AudioClientStatus.Starting:
						return;
					case AudioClientStatus.Closing:
					case AudioClientStatus.Closed:
						throw new ObjectDisposedException(null);
				}
				m_status = AudioClientStatus.Starting;
			}
			try {
				_client.Start();
				lock (_statusLock) m_status = AudioClientStatus.Playing;
			}
			catch (AudioClientDisconnectedException) {
				OnAudioClientDisconnected();
				Start();
			}
		}

		/// <inheritdoc />
		public override void Pause() {
			if (_client == null) throw new InvalidOperationException("Not initialized.");
			lock (_statusLock) {
				switch (m_status) {
					case AudioClientStatus.Idle:
					case AudioClientStatus.Pausing:
						return;
					case AudioClientStatus.Closing:
					case AudioClientStatus.Closed:
						throw new ObjectDisposedException(null);
				}
				m_status = AudioClientStatus.Pausing;
			}
			try {
				_client.Pause();
				lock (_statusLock) m_status = AudioClientStatus.Idle;
			}
			catch (AudioClientDisconnectedException) {
				OnAudioClientDisconnected();
			}
		}

		/// <inheritdoc />
		public override void Close() {
			lock (_statusLock) {
				switch (m_status) {
					case AudioClientStatus.Closing:
					case AudioClientStatus.Closed:
						return;
				}
				m_status = AudioClientStatus.Closing;
			}
			CloseClient();
			_stream?.Dispose();
			CloseDevice();
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		IAudioDevice? _device;
		AudioClient? _client;
		AudioStream? _stream;
		/// <summary>
		/// Selects the audio device.
		/// </summary>
		/// <param name="dataFlow">The data-flow direction.</param>
		/// <returns>The selected audio device.</returns>
		protected virtual IAudioDevice SelectDevice(DataFlow dataFlow) => _manager.GetDefaultDevice(dataFlow);
		/// <summary>
		/// Connects to the audio device.
		/// </summary>
		/// <param name="device">The audio device.</param>
		/// <returns>The audio client.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="device" /> is <see langword="null" />.</exception>
		protected virtual AudioClient ConnectTo(IAudioDevice device) {
			if (device == null) throw new ArgumentNullException(nameof(device));
			return device.Connect(device.DefaultFormat, device.BurstSize + device.MinimumBufferSize);
		}
		/// <summary>
		/// Creates an audio stream as the source stream of the audio client.
		/// </summary>
		/// <returns>The audio stream as the source stream of the audio client.</returns>
		protected abstract AudioStream CreateAudioStream();

		/// <summary>
		/// Initializes the context by connecting to the audio device.
		/// </summary>
		public void Init() {
			ReconnectDevice();
			ReconnectClient();
		}
		void OnAudioClientPlaybackDisconnected(object sender, EventArgs e) {
			OnPlaybackDisconnected();
			OnAudioClientDisconnected();
		}
		void OnAudioClientDisconnected() {
			ReconnectClient();
		}
		void ReconnectClient() {
			CloseClient();
			try {
				Debug.Assert(_device != null);
				if (_device is IAudioClientDevice clientDevice)
					clientDevice.ReactivateClient();
				_client = ConnectTo(_device!);
				if (m_format != _client.Format || m_bufferSize != _client.BufferSize) {
					_stream?.Dispose();
					_stream = CreateAudioStream();
				}
				_client.Source = _stream;
				m_format = _client.Format;
				m_bufferSize = _client.BufferSize;
				_client.PlaybackDisconnected += OnAudioClientPlaybackDisconnected;
				if (m_status == AudioClientStatus.Playing) {
					_client.Start();
				}
			}
			catch (ObjectDisposedException) {
				ReconnectDevice();
				ReconnectClient();
			}
		}
		void ReconnectDevice() {
			CloseDevice();
			_device = SelectDevice(dataFlow);
		}
		void CloseClient() {
			if (_client == null) return;
			_client.PlaybackDisconnected -= OnAudioClientPlaybackDisconnected;
			_client.Close();
		}
		void CloseDevice() {
			if (_device == null) return;
			_device.Dispose();
		}
	}
}
