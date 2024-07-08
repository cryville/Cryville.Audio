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
		static readonly uint SIZE_WAVEHDR = (uint)Marshal.SizeOf(typeof(WAVEHDR));

		internal WaveOutClient(WaveOutDevice device, WaveFormat format, int bufferSize, AudioShareMode shareMode) {
			m_device = device;

			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported.");
			if (bufferSize == 0) bufferSize = device.DefaultBufferSize;

			m_format = format;
			var iformat = Util.ToInternalFormat(format);

			_eventHandle = Synch.CreateEventW(IntPtr.Zero, false, false, null);
			if (_eventHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

			MmSysComExports.MMR(MmeExports.waveOutOpen(
				ref _waveOutHandle, m_device.Index,
				ref iformat, _eventHandle, IntPtr.Zero,
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

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && Playing) Pause();
			if (_waveOutHandle != IntPtr.Zero) {
				MmSysComExports.MMR(MmeExports.waveOutReset(_waveOutHandle));
				for (int i = 0; i < _buffers.Length; i++) {
					_ = MmeExports.waveOutUnprepareHeader(
						_waveOutHandle,
						ref _buffers[i].Header,
						SIZE_WAVEHDR
					);
					_buffers[i].Release();
				}
				MmSysComExports.MMR(MmeExports.waveOutClose(_waveOutHandle));
				_waveOutHandle = IntPtr.Zero;
			}
			if (_eventHandle != IntPtr.Zero) {
				Handle.CloseHandle(_eventHandle);
				_eventHandle = IntPtr.Zero;
			}
		}

		IntPtr _waveOutHandle;
		IntPtr _eventHandle;
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

		MMTIME _time = new MMTIME { wType = (uint)TIME_TYPE.BYTES };
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
			if (!Playing) {
				_thread = new Thread(new ThreadStart(ThreadLogic)) {
					Priority = ThreadPriority.Highest,
					IsBackground = true,
				};
				_thread.Start();
				MmSysComExports.MMR(MmeExports.waveOutRestart(_waveOutHandle));
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
				MmSysComExports.MMR(MmeExports.waveOutPause(_waveOutHandle));
				base.Pause();
			}
		}

		Thread _thread;
		bool _threadAbortFlag;
		void ThreadLogic() {
			_threadAbortFlag = false;
			while (true) {
				for (int i = 0; i < BUFFER_COUNT; i++) {
					var b = _buffers[i];
					if ((b.Header.dwFlags & (uint)WHDR.INQUEUE) == 0) {
						if (Source == null || Muted) {
							Array.Clear(b.Buffer, 0, b.Buffer.Length);
						}
						else {
							Source.Read(b.Buffer, 0, BufferSize * m_format.FrameSize);
						}
						MmSysComExports.MMR(MmeExports.waveOutWrite(_waveOutHandle, ref b.Header, SIZE_WAVEHDR));
						m_bufferPosition += (double)BufferSize / m_format.SampleRate;
					}
				}
				if (_threadAbortFlag) break;
				if (Synch.WaitForSingleObject(_eventHandle, 2000) != /* WAIT_OBJECT_0 */ 0)
					throw new InvalidOperationException("Error while pending for event.");
			}
		}
	}
}
