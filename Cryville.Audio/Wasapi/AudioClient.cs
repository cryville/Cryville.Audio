using Cryville.Common.Platform.Windows;
using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.AudioSessionTypes;
using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using BAudioClient = Cryville.Audio.AudioClient;
using MmUtil = Cryville.Audio.WinMM.Util;

namespace Cryville.Audio.Wasapi {
	public class AudioClient : BAudioClient {
		readonly Internal _internal;
		private class Internal : ComInterfaceWrapper<IAudioClient> {
			public Internal(IAudioClient obj) : base(obj) { }
			public new IAudioClient ComObject => base.ComObject;
		}

		internal AudioClient(IAudioClient obj, MMDevice device) {
			m_device = device;
			_internal = new Internal(obj);
			_internal.ComObject.GetDevicePeriod(out m_defaultBufferDuration, out m_minimumBufferDuration);
		}

		~AudioClient() {
			Dispose(false);
		}

		protected override void Dispose(bool disposing) {
			if (Playing) Pause();
			if (_eventHandle != IntPtr.Zero) Handle.CloseHandle(_eventHandle);
			if (_renderClient != null) _renderClient.Dispose();
			if (_clock != null) Marshal.ReleaseComObject(_clock);
			_internal.Dispose();
		}

		IntPtr _eventHandle;
		AudioRenderClient _renderClient;
		IAudioClock _clock;

		readonly IAudioDevice m_device;
		public override IAudioDevice Device => m_device;

		private readonly long m_defaultBufferDuration;
		public override float DefaultBufferDuration => FromReferenceTime(m_defaultBufferDuration);

		private readonly long m_minimumBufferDuration;
		public override float MinimumBufferDuration => FromReferenceTime(m_minimumBufferDuration);

		public override WaveFormat DefaultFormat {
			get {
				_internal.ComObject.GetMixFormat(out var presult);
				var result = (WAVEFORMATEX)Marshal.PtrToStructure(presult, typeof(WAVEFORMATEX));
				return MmUtil.FromInternalFormat(result);
			}
		}

		private WAVEFORMATEX m_format;
		public override WaveFormat Format {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized");
				return MmUtil.FromInternalFormat(m_format);
			}
		}

		uint m_bufferFrames;
		public override int BufferSize {
			get {
				if (_eventHandle == IntPtr.Zero)
					throw new InvalidOperationException("Connection not initialized");
				return (int)(m_bufferFrames * m_format.nBlockAlign);
			}
		}

		public override float MaximumLatency {
			get {
				_internal.ComObject.GetStreamLatency(out var result);
				return FromReferenceTime(result);
			}
		}

		ulong _clockFreq;
		public override double Position {
			get {
				if (_clockFreq == 0)
					throw new InvalidOperationException("Connection not initialized");
				_clock.GetPosition(out var pos, out _);
				return (double)pos / _clockFreq;
			}
		}

		double m_bufferPosition;
		public override double BufferPosition => m_bufferPosition;

		public override bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			var iformat = MmUtil.ToInternalFormat(format);
			int hr = _internal.ComObject.IsFormatSupported(ToInternalShareModeEnum(shareMode), ref iformat, out var presult);
			if (hr == 0) { // S_OK
				suggestion = format;
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return true;
			}
			else if (hr == 1) { // S_FALSE
				suggestion = MmUtil.FromInternalFormat((WAVEFORMATEX)Marshal.PtrToStructure(presult, typeof(WAVEFORMATEX)));
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return false;
			}
			else if ((hr & 0x7fffffff) == 0x08890008) { // AUDCLNT_E_UNSUPPORTED_FORMAT
				suggestion = null;
				if (presult != IntPtr.Zero) Marshal.FreeCoTaskMem(presult);
				return false;
			}
			else Marshal.ThrowExceptionForHR(hr);
			throw new NotSupportedException("Theoretically unreachable");
		}

		static Guid GUID_AUDIO_CLOCK = typeof(IAudioClock).GUID;
		static Guid GUID_AUDIO_RENDER_CLIENT = typeof(IAudioRenderClient).GUID;
		/// <summary>
		/// Initialize the client.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="bufferDuration">The buffer duration of the connection. The value is clamped to <see cref="MinimumBufferDuration" /> if too small. If <paramref name="shareMode" /> is set to <see cref="AudioShareMode.Shared" />, the buffer duration will be determined automatically despite this parameter.</param>
		/// <param name="shareMode">The share mode of the connection.</param>
		public override void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (shareMode == AudioShareMode.Shared) bufferDuration = 0;
			else if (bufferDuration == 0) bufferDuration = MinimumBufferDuration;
			float period = shareMode == AudioShareMode.Shared ? 0 : bufferDuration;
			m_format = MmUtil.ToInternalFormat(format);

			bool retryFlag = false;
		retry:
			try {
				_internal.ComObject.Initialize(
					ToInternalShareModeEnum(shareMode),
					(uint)AUDCLNT_STREAMFLAGS.EVENTCALLBACK,
					ToReferenceTime(bufferDuration),
					ToReferenceTime(period),
					ref m_format, IntPtr.Zero
				);
			}
			catch (COMException ex) {
				if (!retryFlag && (ex.ErrorCode & 0x7ffffff) == 0x08890019) { // AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
					_internal.ComObject.GetBufferSize(out uint nFrames);
					period = bufferDuration = (long)(1e7 / m_format.nSamplesPerSec * nFrames + 0.5);
					retryFlag = true;
					goto retry;
				}
				else throw;
			}
			_eventHandle = Synch.CreateEventW(IntPtr.Zero, false, false, null);
			if (_eventHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			_internal.ComObject.SetEventHandle(_eventHandle);

			_internal.ComObject.GetService(ref GUID_AUDIO_CLOCK, out var pac);
			_clock = pac as IAudioClock;
			_clock.GetFrequency(out _clockFreq);

			_internal.ComObject.GetBufferSize(out m_bufferFrames);
			if (m_device.DataFlow == DataFlow.Out) {
				_internal.ComObject.GetService(ref GUID_AUDIO_RENDER_CLIENT, out var prc);
				_renderClient = new AudioRenderClient(prc as IAudioRenderClient);
			}
			else
				throw new NotImplementedException();
		}

		public override void Start() {
			if (!Playing) {
				_thread = new Thread(new ThreadStart(ThreadLogic)) {
					Priority = ThreadPriority.Highest,
					IsBackground = true,
				};
				_thread.Start();
				_internal.ComObject.Start();
				base.Start();
			}
		}

		public override void Pause() {
			if (Playing) {
				_internal.ComObject.Stop();
				/*_internal.ComObject.Reset();
				m_bufferPosition = 0;*/
				_thread.Abort();
				_thread = null;
				base.Pause();
			}
		}

		Thread _thread;

		void ThreadLogic() {
			var buffer = new byte[BufferSize];
			while (true) {
				if (Synch.WaitForSingleObject(_eventHandle, 2000) != /* WAIT_OBJECT_0 */ 0)
					throw new InvalidOperationException("Error while pending for event");
				_internal.ComObject.GetCurrentPadding(out var padding);
				var frames = m_bufferFrames - padding;
				if (frames == 0) continue;
				if (Source == null || Source.Muted) {
					_renderClient.SilentBuffer(frames);
				}
				else {
					var length = frames * m_format.nBlockAlign;
					Source.FillBuffer(buffer, 0, (int)length);
					_renderClient.FillBuffer(buffer, frames, length);
					_renderClient.ReleaseBuffer(frames);
				}
				m_bufferPosition += (double)frames / m_format.nSamplesPerSec;
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
