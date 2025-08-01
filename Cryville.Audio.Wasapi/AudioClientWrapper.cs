using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.MmReg;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="AudioClient" /> that interact with Wasapi.
	/// </summary>
	public class AudioClientWrapper : AudioClient {
		readonly IAudioClient _internal;

		internal AudioClientWrapper(IAudioClient obj, MMDeviceWrapper device, WaveFormat format, int bufferSize, AudioUsage usage, AudioShareMode shareMode) {
			m_device = device;
			_internal = obj;

			if (shareMode == AudioShareMode.Shared) bufferSize = 0;
			else if (bufferSize == 0) bufferSize = device.DefaultBufferSize;
			m_format = Helpers.ToInternalFormat(format);

			var bufferDuration = Helpers.ToReferenceTime(format.SampleRate, bufferSize);
			bool retryFlag = false;
		retry:
			try {
				_internal.Initialize(
					ToInternalShareModeEnum(shareMode),
					(uint)AUDCLNT_STREAMFLAGS.EVENTCALLBACK,
					bufferDuration, bufferDuration,
					ref m_format, IntPtr.Zero
				);
			}
			catch (COMException ex) when ((ex.ErrorCode & 0x7ffffff) == 0x08890019) { // AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
				if (retryFlag) throw;
				retryFlag = true;
				_internal.GetBufferSize(out uint nFrames);
				bufferDuration = (long)(1e7 / m_format.Format.nSamplesPerSec * nFrames + 0.5);
				goto retry;
			}

			if (_internal is IAudioClient2 client2) {
				var props = new AudioClientProperties() { eCategory = Helpers.ToInternalStreamCategory(usage) };
				client2.SetClientProperties(ref props);
			}

			_eventHandle = Synch.CreateEventW(IntPtr.Zero, false, false, null);
			if (_eventHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			_internal.SetEventHandle(_eventHandle);

			_internal.GetService(typeof(IAudioClock).GUID, out var clock);
			_clock = (IAudioClock)clock;
			_clock.GetFrequency(out _clockFreq);

			_internal.GetBufferSize(out m_bufferFrames);
			if (m_device.DataFlow == DataFlow.Out) {
				_internal.GetService(typeof(IAudioRenderClient).GUID, out var prc);
				_renderClient = new AudioRenderClientWrapper((IAudioRenderClient)prc);
			}
			else
				throw new NotImplementedException();
		}

		/// <inheritdoc />
		~AudioClientWrapper() {
			Dispose(false);
		}
		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			CloseNative();
		}

		IntPtr _eventHandle;
		readonly AudioRenderClientWrapper _renderClient;
		readonly IAudioClock _clock;

		readonly MMDeviceWrapper m_device;
		/// <inheritdoc />
		public override IAudioDevice Device => m_device;

		private WAVEFORMATEXTENSIBLE m_format;
		/// <inheritdoc />
		public override WaveFormat Format {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized.");
				return Helpers.FromInternalFormat(m_format);
			}
		}

		readonly uint m_bufferFrames;
		/// <inheritdoc />
		public override int BufferSize {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized.");
				return (int)m_bufferFrames;
			}
		}

		/// <inheritdoc />
		public override float MaximumLatency {
			get {
				try {
					_internal.GetStreamLatency(out var result);
					return result / 1e4f;
				}
				catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
					lock (_statusLock) m_status = AudioClientStatus.Disconnected;
					throw new AudioClientDisconnectedException(ex);
				}
			}
		}

		readonly object _statusLock = new();
		volatile AudioClientStatus m_status;
		/// <inheritdoc />
		public override AudioClientStatus Status => m_status;

		readonly ulong _clockFreq;
		/// <inheritdoc />
		public override double Position {
			get {
				if (_clockFreq == 0)
					throw new InvalidOperationException("Connection not initialized.");
				try {
					_clock.GetPosition(out var pos, out _);
					return (double)pos / _clockFreq;
				}
				catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
					lock (_statusLock) m_status = AudioClientStatus.Disconnected;
					throw new AudioClientDisconnectedException(ex);
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
			var thread = new Thread(new ThreadStart(ThreadLogic)) {
				Priority = ThreadPriority.Highest,
				IsBackground = true,
				Name = "WASAPI audio rendering thread",
			};
			thread.Start();
			_thread = thread;
			try {
				_internal.Start();
			}
			catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
				lock (_statusLock) m_status = AudioClientStatus.Disconnected;
				throw new AudioClientDisconnectedException(ex);
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
			StopPlaybackThread();
			try {
				_internal.Stop();
			}
			catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
				lock (_statusLock) m_status = AudioClientStatus.Disconnected;
				throw new AudioClientDisconnectedException(ex);
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
			StopPlaybackThread();
			CloseNative();
			_renderClient?.Dispose();
			if (_clock != null) Marshal.ReleaseComObject(_clock);
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		void CloseNative() {
			IntPtr eventHandle = Interlocked.Exchange(ref _eventHandle, IntPtr.Zero);
			if (eventHandle != IntPtr.Zero) {
				Handle.CloseHandle(eventHandle);
			}
		}

		void StopPlaybackThread() {
			_threadAbortFlag = true;
			if (!(_thread?.Join(1000) ?? true))
				throw new InvalidOperationException("Failed to pause audio client.");
		}

		Thread? _thread;
		bool _threadAbortFlag;
		void ThreadLogic() {
			_threadAbortFlag = false;
			try {
				while (true) {
					if (Synch.WaitForSingleObject(_eventHandle, 2000) != /* WAIT_OBJECT_0 */ 0)
						throw new InvalidOperationException("Error while pending for event.");
					_internal.GetCurrentPadding(out var padding);
					var frames = m_bufferFrames - padding;
					if (frames == 0) continue;
					if (Source == null) {
						_renderClient.SilentBuffer(frames);
					}
					else {
						ref byte buffer = ref _renderClient.GetBuffer(frames);
						Source.ReadFrames(ref buffer, (int)frames);
						_renderClient.ReleaseBuffer(frames);
						Source.ReadFramesGreedily(ref buffer, (int)frames);
					}
					m_bufferPosition += (double)frames / m_format.Format.nSamplesPerSec;
					if (_threadAbortFlag) break;
				}
			}
			catch (COMException ex) when ((uint)ex.ErrorCode == 0x88890004) {
				lock (_statusLock) m_status = AudioClientStatus.Disconnected;
				// Launch a new thread to handle the disconnection in case of deadlock
				var thread = new Thread(OnPlaybackDisconnected) {
					IsBackground = true,
					Name = "AudioClient disconnection handler",
				};
				thread.Start();
			}
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) => value switch {
			AudioShareMode.Shared => AUDCLNT_SHAREMODE.SHARED,
			AudioShareMode.Exclusive => AUDCLNT_SHAREMODE.EXCLUSIVE,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}
}
