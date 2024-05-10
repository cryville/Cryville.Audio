using Android.AAudio.Native;
using Cryville.Common.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cryville.Audio.AAudio {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with AAudio.
	/// </summary>
	[SuppressMessage("Naming", "CA1711")]
	public class AAudioStream : AudioClient {
		static readonly Dictionary<IntPtr, AAudioStream> _instances = [];

		readonly AAudioStreamBuilder _builder;
		readonly IntPtr _stream;

		internal AAudioStream(AAudioStreamBuilder builder, IntPtr stream) {
			_builder = builder;
			_stream = stream;
			m_format = Util.FromInternalWaveFormat(stream);
			_buffer = new byte[BufferSize * m_format.FrameSize];
			_instances.Add(_stream, this);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (Playing) Pause();
				_instances.Remove(_stream);
			}
			UnsafeNativeMethods.AAudioStream_close(_stream);
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

		/// <inheritdoc />
		public override double Position {
			get {
				UnsafeNativeMethods.AAudioStream_getTimestamp(_stream, clockid_t.CLOCK_MONOTONIC, out var frames, out _);
				return frames / (double)Format.SampleRate;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		/// <inheritdoc />
		public override void Start() {
			if (!Playing) {
				UnsafeNativeMethods.AAudioStream_requestStart(_stream);
				base.Start();
			}
		}

		/// <inheritdoc />
		public override void Pause() {
			if (Playing) {
				UnsafeNativeMethods.AAudioStream_requestPause(_stream);
				UnsafeNativeMethods.AAudioStream_waitForStateChange(_stream, aaudio_stream_state_t.AAUDIO_STREAM_STATE_PAUSING, out _, 2000000000);
				base.Pause();
			}
		}

		readonly byte[] _buffer;
		unsafe void FillBuffer(IntPtr ptr, int frames) {
			var len = frames * Format.FrameSize;
			if (Source == null || Muted) Array.Clear(_buffer, 0, len);
			else Source.Read(_buffer, 0, len);
			Marshal.Copy(_buffer, 0, ptr, len);
			m_bufferPosition += (double)frames / Format.SampleRate;
		}

		[MonoPInvokeCallback]
		internal static unsafe aaudio_data_callback_result_t DataCallback(IntPtr stream, IntPtr _, IntPtr audioData, int numFrames) {
			if (!_instances.TryGetValue(stream, out var instance))
				return aaudio_data_callback_result_t.AAUDIO_CALLBACK_RESULT_STOP;
			instance.FillBuffer(audioData, numFrames);
			return aaudio_data_callback_result_t.AAUDIO_CALLBACK_RESULT_CONTINUE;
		}
	}
}
