using Microsoft.Windows;
using Microsoft.Windows.Mme;
using Microsoft.Windows.MmSysCom;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.WaveformAudio {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with WinMM.
	/// </summary>
	public class WaveOutClient : AudioClient {
		const int BUFFER_COUNT = 2;
#if NET451_OR_GREATER || NETSTANDARD1_2_OR_GREATER || NETCOREAPP1_0_OR_GREATER
		static readonly uint SIZE_WAVEHDR = (uint)Marshal.SizeOf<WAVEHDR>();
#else
		static readonly uint SIZE_WAVEHDR = (uint)Marshal.SizeOf(typeof(WAVEHDR));
#endif

		internal WaveOutClient(WaveOutDevice device, WaveFormat format, int bufferSize, AudioShareMode shareMode) {
			m_device = device;

			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported.");
			if (bufferSize == 0) bufferSize = device.DefaultBufferSize;

			m_format = format;
			var iFormat = Helpers.ToInternalFormat(format);

			_eventHandle = new(false);

			MmSysComExports.MMR(MmeExports.waveOutOpen(
				ref _waveOutHandle, m_device.Index,
				ref iFormat, _eventHandle.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero,
				(uint)CALLBACK_TYPE.CALLBACK_EVENT
			));

			m_bufferSize = bufferSize;
			_buffers = new WaveBuffer[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; i++) {
				_buffers[i] = new WaveBuffer((uint)(m_bufferSize * format.FrameSize));
				MmSysComExports.MMR(MmeExports.waveOutPrepareHeader(
					_waveOutHandle,
					ref _buffers[i].Header,
					SIZE_WAVEHDR
				));
			}
		}

		/// <inheritdoc />
		~WaveOutClient() {
			Dispose(false);
		}
		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			CloseNative();
		}

		IntPtr _waveOutHandle;
		readonly AutoResetEvent _eventHandle;
		readonly WaveBuffer[] _buffers;

		readonly WaveOutDevice m_device;
		/// <inheritdoc />
		public override IAudioDevice Device => m_device;

		readonly WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format => m_format;

		readonly int m_bufferSize;
		/// <inheritdoc />
		public override int BufferSize => m_bufferSize;

		/// <inheritdoc />
		public override float MaximumLatency => 0;

		readonly object _statusLock = new();
		volatile AudioClientStatus m_status;
		/// <inheritdoc />
		public override AudioClientStatus Status => m_status;

		MMTIME _time = new() { wType = (uint)TIME_TYPE.BYTES };
		/// <inheritdoc />
		public override double Position {
			get {
				MmSysComExports.MMR(MmeExports.waveOutGetPosition(_waveOutHandle, ref _time, (uint)Marshal.SizeOf(_time)));
				return (double)(uint)_time.Value / m_format.BytesPerSecond;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		/// <inheritdoc />
		public override void Start() {
			if (_thread != null) return;
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
				Name = "WaveformAudio audio rendering thread",
			};
			thread.Start();
			_thread = thread;
			MmSysComExports.MMR(MmeExports.waveOutRestart(_waveOutHandle));
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
			thread.Interrupt();
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
			_thread?.Interrupt();
			_thread?.Join(4000);
			CloseNative();
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		void CloseNative() {
			IntPtr waveOutHandle = Interlocked.Exchange(ref _waveOutHandle, IntPtr.Zero);
			if (waveOutHandle != IntPtr.Zero) {
				MmSysComExports.MMR(MmeExports.waveOutReset(waveOutHandle));
				foreach (var b in _buffers) {
					_ = MmeExports.waveOutUnprepareHeader(
						waveOutHandle,
						ref b.Header,
						SIZE_WAVEHDR
					);
					b.Release();
				}
				MmSysComExports.MMR(MmeExports.waveOutClose(waveOutHandle));
			}
			_eventHandle.Close();
		}

		Thread? _thread;
		void ThreadLogic() {
			uint waitThreshold = Math.Max(1000, (uint)BufferSize / Format.SampleRate * 2000);
			try {
				while (true) {
					foreach (var b in _buffers) {
						if ((b.Header.dwFlags & (uint)WHDR.INQUEUE) == 0) {
							if (Stream == null) {
								AudioStream.SilentBuffer(Format, ref b.Buffer[0], BufferSize);
							}
							else {
								Stream.ReadFramesGreedily(b.Buffer, 0, BufferSize);
							}
							MmSysComExports.MMR(MmeExports.waveOutWrite(handle, ref b.Header, SIZE_WAVEHDR));
							m_bufferPosition += (double)BufferSize / m_format.SampleRate;
						}
					}
					if (!_eventHandle.WaitOne(waitThreshold))
						throw new TimeoutException("Timed out while pending for event.");
				}
			}
			catch (ThreadInterruptedException) {
				// Wait for all the buffers to be returned, to avoid dead buffers
				for (; ; ) {
					foreach (var b in _buffers) {
						if ((b.Header.dwFlags & (uint)WHDR.INQUEUE) != 0) {
							goto waitForNextBuffer;
						}
					}
					break;
				waitForNextBuffer:
					_eventHandle.WaitOne(waitThreshold);
				}
				MmSysComExports.MMR(MmeExports.waveOutPause(handle));
				lock (_statusLock) if (m_status == AudioClientStatus.Pausing) m_status = AudioClientStatus.Idle;
			}
		}
	}
}
