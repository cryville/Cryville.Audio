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
	/// <para>Be cautious when replacing the audio stream with <see cref="AudioClient.Source" />. If the audio stream needs to recreated, the replaced audio stream will be discarded, and <see cref="CreateAudioStream" /> will be called to create the new stream.</para>
	/// <para>The caller must not dispose <see cref="MainStream" />, and <see cref="AudioClient.Source" /> as well if it was not replaced. The caller is responsible for disposing any replaced <see cref="AudioClient.Source" />.</para>
	/// </remarks>
	public abstract class AutoReconnectContext(IAudioDeviceManager manager, DataFlow dataFlow) : AudioClient {
		[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
		[SuppressMessage("Usage", "CA2213", Justification = "Not owned")]
		readonly IAudioDeviceManager _manager = manager ?? throw new ArgumentNullException(nameof(manager));

		/// <inheritdoc />
		public override IAudioDevice Device => Client.Device;

		WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format { get { _ = Client; return m_format; } }

		int m_bufferSize;
		/// <inheritdoc />
		public override int BufferSize { get { _ = Client; return m_bufferSize; } }

		/// <inheritdoc />
		public override float MaximumLatency => Client.MaximumLatency;

		readonly object _statusLock = new();
		volatile AudioClientStatus m_status;
		/// <inheritdoc />
		public override AudioClientStatus Status => m_status;

		/// <inheritdoc />
		public override double Position {
			get {
				try {
					return Client.Position;
				}
				catch (AudioClientDisconnectedException) {
					OnAudioClientDisconnected();
					return Position;
				}
			}
		}

		/// <inheritdoc />
		public override double BufferPosition => Client.BufferPosition;

		/// <inheritdoc />
		protected override void OnSetSource() => Client.Source = Source;

		/// <inheritdoc />
		public override void Start() {
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
				Client.Start();
				lock (_statusLock) m_status = AudioClientStatus.Playing;
			}
			catch (AudioClientDisconnectedException) {
				OnAudioClientDisconnected();
				Start();
			}
		}

		/// <inheritdoc />
		public override void Pause() {
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
				Client.Pause();
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
			MainStream?.Dispose();
			CloseDevice();
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		IAudioDevice? _device;
		AudioClient? _client;
		AudioClient Client => _client ?? throw new InvalidOperationException("Not initialized.");
		/// <summary>
		/// The stream created with <see cref="CreateAudioStream" />.
		/// </summary>
		/// <remarks>
		/// <para>The audio stream held by this property is owned by the context and must not be disposed by the caller.</para>
		/// </remarks>
		public AudioStream? MainStream { get; private set; }
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
		/// <remarks>
		/// <para>This method is called when the context is initialized or when the audio stream needs to be recreated after reconnection. For the latter case, the caller can use <see cref="MainStream" /> to capture the state of the last audio stream. When this method returns, the last stream will be disposed and <see cref="MainStream" /> will be replaced with the new stream returned by this method.</para>
		/// <para>The audio stream needs to be recreated after reconnection if the format or the buffer size of the audio client is changed.</para>
		/// </remarks>
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
					m_format = _client.Format;
					m_bufferSize = _client.BufferSize;
					var newStream = CreateAudioStream();
					MainStream?.Dispose();
					Source = MainStream = newStream;
				}
				else {
					OnSetSource();
				}
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
