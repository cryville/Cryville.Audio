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
		IntPtr _stream;

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
		public override int BufferSize {
			get {
				IntPtr stream = _stream;
				if (stream == IntPtr.Zero) throw new ObjectDisposedException(null);
				return UnsafeNativeMethods.AAudioStream_getBufferSizeInFrames(stream);
			}
		}

		/// <inheritdoc />
		public override float MaximumLatency => 0;

		/// <inheritdoc />
		public override AudioClientStatus Status {
			get {
				IntPtr stream = _stream;
				if (stream == IntPtr.Zero) return AudioClientStatus.Closed;
				return FromInternalState(UnsafeNativeMethods.AAudioStream_getState(stream));
			}
		}

		static AudioClientStatus FromInternalState(aaudio_stream_state_t state) {
			return state switch {
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_OPEN or
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSED or
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_FLUSHING or
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_FLUSHED or
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_STOPPED => AudioClientStatus.Idle,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTING => AudioClientStatus.Starting,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTED => AudioClientStatus.Playing,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSING or
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_STOPPING => AudioClientStatus.Pausing,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_CLOSING => AudioClientStatus.Closing,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_CLOSED => AudioClientStatus.Closed,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_DISCONNECTED => AudioClientStatus.Disconnected,
				aaudio_stream_state_t.AAUDIO_STREAM_STATE_UNINITIALIZED => throw new ObjectDisposedException(null),
				_ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unknown AAudio state: {0}.", state)),
			};
		}

		/// <inheritdoc />
		public override double Position {
			get {
				IntPtr stream = _stream;
				if (stream == IntPtr.Zero) throw new ObjectDisposedException(null);
				Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_getTimestamp(stream, clockid_t.CLOCK_MONOTONIC, out var frames, out _));
				return frames / (double)Format.SampleRate;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		bool _started;
		/// <inheritdoc />
		public override bool WaitForNextStatus(AudioClientStatus currentStatus, out AudioClientStatus newStatus, TimeSpan timeout) {
			IntPtr stream = _stream;
			if (stream == IntPtr.Zero) {
				newStatus = AudioClientStatus.Closed;
				return currentStatus != AudioClientStatus.Closed;
			}
			var result = UnsafeNativeMethods.AAudioStream_waitForStateChange(stream, currentStatus switch {
				AudioClientStatus.Idle => _started ? aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSED : aaudio_stream_state_t.AAUDIO_STREAM_STATE_OPEN,
				AudioClientStatus.Starting => aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTING,
				AudioClientStatus.Playing => aaudio_stream_state_t.AAUDIO_STREAM_STATE_STARTED,
				AudioClientStatus.Pausing => aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSING,
				AudioClientStatus.Closing => aaudio_stream_state_t.AAUDIO_STREAM_STATE_CLOSING,
				AudioClientStatus.Closed => aaudio_stream_state_t.AAUDIO_STREAM_STATE_CLOSED,
				AudioClientStatus.Disconnected => aaudio_stream_state_t.AAUDIO_STREAM_STATE_DISCONNECTED,
				_ => throw new NotImplementedException(),
			}, out var nextState, (long)(timeout.TotalMilliseconds * 1000));
			if (result == aaudio_result_t.AAUDIO_ERROR_TIMEOUT) {
				newStatus = currentStatus;
				return false;
			}
			Helpers.ThrowIfError(result);
			newStatus = FromInternalState(nextState);
			return true;
		}

		/// <inheritdoc />
		public override void RequestStart() {
			IntPtr stream = _stream;
			if (stream == IntPtr.Zero) throw new ObjectDisposedException(null);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_requestStart(stream));
			_started = true;
		}

		/// <inheritdoc />
		public override void RequestPause() {
			IntPtr stream = _stream;
			if (stream == IntPtr.Zero) throw new ObjectDisposedException(null);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_requestPause(stream));
		}

		/// <inheritdoc />
		public override void Close() {
			IntPtr stream = Interlocked.Exchange(ref _stream, IntPtr.Zero);
			if (stream == IntPtr.Zero) return;
			_instances.Remove(stream);
			Helpers.ThrowIfError(UnsafeNativeMethods.AAudioStream_close(stream));
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
			var thread = new Thread(instance.OnPlaybackDisconnected) {
				IsBackground = true,
				Name = "AAudioStream disconnection handler",
			};
			thread.Start();
		}
	}
}
