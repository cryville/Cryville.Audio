using Cryville.Audio.AAudio.Native;
using Cryville.Interop.Mono;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using UnsafeIL;

namespace Cryville.Audio.AAudio {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with AAudio.
	/// </summary>
	[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
	[SuppressMessage("Naming", "CA1711", Justification = "[sic]")]
	public class AAudioStream : AudioClient {
		static readonly Dictionary<IntPtr, AAudioStream> _instances = [];

		readonly AAudioStreamBuilder _builder;
		readonly IntPtr _stream;

		internal AAudioStream(AAudioStreamBuilder builder, IntPtr stream) {
			_builder = builder;
			_stream = stream;
			m_format = Helpers.FromInternalWaveFormat(stream);
			_instances.Add(_stream, this);
		}

		/// <inheritdoc />
		public override IAudioDevice Device => _builder;

		readonly WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format => m_format;

		/// <inheritdoc />
		public override int BufferSize => UnsafeNativeMethods.AAudioStream_getBufferSizeInFrames(_stream);

		/// <inheritdoc />
		public override float MaximumLatency => 0;

		readonly object _statusLock = new();
		volatile AudioClientStatus m_status;
		/// <inheritdoc />
		public override AudioClientStatus Status => m_status;

		/// <inheritdoc />
		public override double Position {
			get {
				try {
					Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_getTimestamp(_stream, clockid_t.CLOCK_MONOTONIC, out var frames, out _));
					return frames / (double)Format.SampleRate;
				}
				catch (AudioClientDisconnectedException) {
					lock (_statusLock) m_status = AudioClientStatus.Disconnected;
					throw;
				}
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

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
				Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_requestStart(_stream));
				Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_waitForStateChange(_stream, aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTING, out var state, 2000000000));
				if (state == aaudio_stream_state_t.AAUDIO_STREAM_STATE_DISCONNECTED) throw new AudioClientDisconnectedException();
				if (state != aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTED) throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to start the audio client. State: {0}", state));
			}
			catch (AudioClientDisconnectedException) {
				lock (_statusLock) m_status = AudioClientStatus.Disconnected;
				throw;
			}
			lock (_statusLock) m_status = AudioClientStatus.Playing;
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
				Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_requestPause(_stream));
				Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_waitForStateChange(_stream, aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSING, out var state, 2000000000));
				if (state == aaudio_stream_state_t.AAUDIO_STREAM_STATE_DISCONNECTED) throw new AudioClientDisconnectedException();
				if (state != aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSED) throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to pause the audio client. State: {0}", state));
			}
			catch (AudioClientDisconnectedException) {
				lock (_statusLock) m_status = AudioClientStatus.Disconnected;
				throw;
			}
			lock (_statusLock) m_status = AudioClientStatus.Idle;
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
			_instances.Remove(_stream);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_close(_stream));
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		unsafe void FillBuffer(IntPtr ptr, int frames) {
			ref byte buffer = ref Unsafe.AsRef<byte>((void*)ptr);
			if (Stream == null) AudioStream.SilentBuffer(Format, ref buffer, frames);
			else Stream.ReadFramesGreedily(ref buffer, frames);
			m_bufferPosition += (double)frames / Format.SampleRate;
		}

		[MonoPInvokeCallback(typeof(AAudioStream_dataCallback))]
		internal static unsafe aaudio_data_callback_result_t DataCallback(IntPtr stream, IntPtr _, IntPtr audioData, int numFrames) {
			if (!_instances.TryGetValue(stream, out var instance))
				return aaudio_data_callback_result_t.AAUDIO_CALLBACK_RESULT_STOP;
			instance.FillBuffer(audioData, numFrames);
			return aaudio_data_callback_result_t.AAUDIO_CALLBACK_RESULT_CONTINUE;
		}

		[MonoPInvokeCallback(typeof(AAudioStream_errorCallback))]
		internal static void ErrorCallback(IntPtr stream, IntPtr _, aaudio_result_t _2) {
			if (!_instances.TryGetValue(stream, out var instance)) 
				return;
			// Launch a new thread to handle the disconnection in case of deadlock
			lock (instance._statusLock) instance.m_status = AudioClientStatus.Disconnected;
			var thread = new Thread(instance.OnPlaybackDisconnected) {
				IsBackground = true,
				Name = "AAudioStream disconnection handler",
			};
			thread.Start();
		}
	}
}
