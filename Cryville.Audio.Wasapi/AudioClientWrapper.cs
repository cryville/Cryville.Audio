using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.Wasapi {
	/// <summary>
	/// An <see cref="AudioClient" /> that interact with Wasapi.
	/// </summary>
	public class AudioClientWrapper : AudioClient {
		IAudioClient _internal;

		static Guid GUID_AUDIO_CLOCK = new Guid("CD63314F-3FBA-4a1b-812C-EF96358728E7");
		static Guid GUID_AUDIO_RENDER_CLIENT = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");
		internal AudioClientWrapper(IAudioClient obj, MMDeviceWrapper device, WaveFormat format, float bufferDuration, AudioShareMode shareMode) {
			m_device = device;
			_internal = obj;

			if (shareMode == AudioShareMode.Shared) bufferDuration = 0;
			else if (bufferDuration == 0) bufferDuration = device.MinimumBufferDuration;
			float period = shareMode == AudioShareMode.Shared ? 0 : bufferDuration;
			m_format = Util.ToInternalFormat(format);

			bool retryFlag = false;
		retry:
			try {
				_internal.Initialize(
					ToInternalShareModeEnum(shareMode),
					(uint)AUDCLNT_STREAMFLAGS.EVENTCALLBACK,
					ToReferenceTime(bufferDuration),
					ToReferenceTime(period),
					ref m_format, IntPtr.Zero
				);
			}
			catch (COMException ex) {
				if (!retryFlag && (ex.ErrorCode & 0x7ffffff) == 0x08890019) { // AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
					_internal.GetBufferSize(out uint nFrames);
					period = bufferDuration = (long)(1e7 / m_format.nSamplesPerSec * nFrames + 0.5);
					retryFlag = true;
					goto retry;
				}
				else throw;
			}
			_eventHandle = Synch.CreateEventW(IntPtr.Zero, false, false, null);
			if (_eventHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			_internal.SetEventHandle(_eventHandle);

			_internal.GetService(ref GUID_AUDIO_CLOCK, out var clock);
			_clock = clock as IAudioClock;
			_clock.GetFrequency(out _clockFreq);

			_internal.GetBufferSize(out m_bufferFrames);
			if (m_device.DataFlow == DataFlow.Out) {
				_internal.GetService(ref GUID_AUDIO_RENDER_CLIENT, out var prc);
				_renderClient = new AudioRenderClientWrapper(prc as IAudioRenderClient);
			}
			else
				throw new NotImplementedException();
		}

		/// <inheritdoc />
		~AudioClientWrapper() {
			Dispose(false);
		}

		int _disposeCount;
		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			if (Interlocked.Increment(ref _disposeCount) == 1) {
				if (Playing) Pause();
				if (_eventHandle != IntPtr.Zero) {
					Handle.CloseHandle(_eventHandle);
					_eventHandle = IntPtr.Zero;
				}
				if (_renderClient != null) {
					_renderClient.Dispose();
					_renderClient = null;
				}
				if (_clock != null && disposing) {
					Marshal.ReleaseComObject(_clock);
					_clock = null;
				}
				if (_internal != null && disposing) {
					Marshal.ReleaseComObject(_internal);
					_internal = null;
				}
			}
		}

		IntPtr _eventHandle;
		AudioRenderClientWrapper _renderClient;
		IAudioClock _clock;

		readonly IAudioDevice m_device;
		/// <inheritdoc />
		public override IAudioDevice Device => m_device;

		private WAVEFORMATEX m_format;
		/// <inheritdoc />
		public override WaveFormat Format {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized.");
				return Util.FromInternalFormat(m_format);
			}
		}

		readonly uint m_bufferFrames;
		/// <inheritdoc />
		public override int BufferSize {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized.");
				return (int)(m_bufferFrames * m_format.nBlockAlign);
			}
		}

		/// <inheritdoc />
		public override float MaximumLatency {
			get {
				_internal.GetStreamLatency(out var result);
				return FromReferenceTime(result);
			}
		}

		readonly ulong _clockFreq;
		/// <inheritdoc />
		public override double Position {
			get {
				if (_clockFreq == 0)
					throw new InvalidOperationException("Connection not initialized.");
				_clock.GetPosition(out var pos, out _);
				return (double)pos / _clockFreq;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		/// <inheritdoc />
		public override void Start() {
			if (!Playing) {
				_thread = new Thread(new ThreadStart(ThreadLogic)) {
					Priority = ThreadPriority.Highest,
					IsBackground = true,
				};
				_thread.Start();
				_internal.Start();
				base.Start();
			}
		}

		/// <inheritdoc />
		public override void Pause() {
			if (Playing) {
				_threadAbortFlag = true;
				if (!_thread.Join(1000))
					throw new InvalidOperationException("Failed to pause audio client.");
				_thread = null;
				_internal.Stop();
				base.Pause();
			}
		}

		Thread _thread;
		bool _threadAbortFlag;
		void ThreadLogic() {
			_threadAbortFlag = false;
			var buffer = new byte[BufferSize];
			while (true) {
				if (Synch.WaitForSingleObject(_eventHandle, 2000) != /* WAIT_OBJECT_0 */ 0)
					throw new InvalidOperationException("Error while pending for event.");
				_internal.GetCurrentPadding(out var padding);
				var frames = m_bufferFrames - padding;
				if (frames == 0) continue;
				if (Source == null || Muted) {
					_renderClient.SilentBuffer(frames);
				}
				else {
					var length = frames * m_format.nBlockAlign;
					Source.Read(buffer, 0, (int)length);
					_renderClient.FillBuffer(buffer, frames, length);
					_renderClient.ReleaseBuffer(frames);
				}
				m_bufferPosition += (double)frames / m_format.nSamplesPerSec;
				if (_threadAbortFlag) break;
			}
		}

		static AUDCLNT_SHAREMODE ToInternalShareModeEnum(AudioShareMode value) {
			switch (value) {
				case AudioShareMode.Shared: return AUDCLNT_SHAREMODE.SHARED;
				case AudioShareMode.Exclusive: return AUDCLNT_SHAREMODE.EXCLUSIVE;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}

		static long ToReferenceTime(float value) {
			return (long)(value * 1e4);
		}
		static float FromReferenceTime(long value) {
			return value / 1e4f;
		}
	}
}