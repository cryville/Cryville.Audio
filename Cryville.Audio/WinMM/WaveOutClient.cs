using Microsoft.Windows;
using Microsoft.Windows.Mme;
using Microsoft.Windows.MmSysCom;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.WinMM {
	public class WaveOutClient : AudioClient {
		internal WaveOutClient(WaveOutDevice device) {
			m_device = device;
		}

		protected override void Dispose(bool disposing) {
			if (_waveOutHandle != IntPtr.Zero) {
				if (Playing) Pause();
				MmSysComExports.MMR(MmeExports.waveOutReset(_waveOutHandle));
				for (int i = 0; i < _buffers.Length; i++) {
					// while ((_buffers[i].Header.dwFlags & (uint)WHDR.DONE) == 0) Thread.Sleep(10);
					MmSysComExports.MMR(MmeExports.waveOutUnprepareHeader(
						_waveOutHandle,
						ref _buffers[i].Header,
						SIZE_WAVEHDR
					));
					_buffers[i].Dispose();
				}
				MmSysComExports.MMR(MmeExports.waveOutClose(_waveOutHandle));
			}
			if (_eventHandle != IntPtr.Zero) Handle.CloseHandle(_eventHandle);
		}

		IntPtr _waveOutHandle;
		IntPtr _eventHandle;
		WaveBuffer[] _buffers;

		readonly WaveOutDevice m_device;
		public override IAudioDevice Device => m_device;

		public override float DefaultBufferDuration => 40;

		public override float MinimumBufferDuration => 0;

		public override WaveFormat DefaultFormat {
			get {
				IsFormatSupported(
					new WaveFormat { Channels = 2, SampleRate = 192000, BitsPerSample = 64 },
					out var format
				);
				return format ?? throw new NotSupportedException("No format is supported");
			}
		}

		WAVEFORMATEX _format;
		WaveFormat m_format;
		public override WaveFormat Format => m_format;

		int m_bufferSize;
		public override int BufferSize => m_bufferSize;

		public override float MaximumLatency => 0;

		MMTIME _time = new MMTIME { wType = (uint)TIME_TYPE.BYTES };
		public override double Position {
			get {
				MmSysComExports.MMR(MmeExports.waveOutGetPosition(_waveOutHandle, ref _time, (uint)Marshal.SizeOf(_time)));
				return (double)(uint)_time.Value / m_format.BytesPerSecond;
			}
		}

		double m_bufferPosition;
		public override double BufferPosition => m_bufferPosition;

		const int BUFFER_COUNT = 3;
		readonly static uint SIZE_WAVEHDR = (uint)Marshal.SizeOf(typeof(WAVEHDR));
		public override void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported");
			if (bufferDuration == 0) bufferDuration = DefaultBufferDuration;

			m_format = format;
			_format = Util.ToInternalFormat(format);

			_eventHandle = Synch.CreateEventW(IntPtr.Zero, false, false, null);
			if (_eventHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

			MmSysComExports.MMR(MmeExports.waveOutOpen(
				ref _waveOutHandle, m_device.Index,
				ref _format, _eventHandle, IntPtr.Zero,
				(uint)CALLBACK_TYPE.CALLBACK_EVENT
			));

			m_bufferSize = format.Align(bufferDuration / 1000 * format.BytesPerSecond);
			_buffers = new WaveBuffer[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; i++) {
				_buffers[i] = new WaveBuffer((uint)m_bufferSize);
				MmSysComExports.MMR(MmeExports.waveOutPrepareHeader(
					_waveOutHandle,
					ref _buffers[i].Header,
					SIZE_WAVEHDR
				));
			}
		}

		public override bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			ushort ch = format.Channels;
			byte flagch;
			switch (ch) {
				case 1: flagch = 0; break;
				case 2: flagch = 1; break;
				default:
					format.Channels = 2;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			uint sr = format.SampleRate;
			byte flagsr;
			switch (sr) {
				case 11025: flagsr = 0; break;
				case 22050: flagsr = 1; break;
				case 44100: flagsr = 2; break;
				case 48000: flagsr = 3; break;
				case 96000: flagsr = 4; break;
				default:
					if      (sr <= 11025) format.SampleRate = 11025;
					else if (sr <= 22050) format.SampleRate = 22050;
					else if (sr <= 44100) format.SampleRate = 44100;
					else if (sr <= 48000) format.SampleRate = 48000;
					else                  format.SampleRate = 96000;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			ushort bits = format.BitsPerSample;
			byte flagbits;
			switch (bits) {
				case  8: flagbits = 0; break;
				case 16: flagbits = 1; break;
				default:
					if (bits <= 8) format.BitsPerSample =  8;
					else           format.BitsPerSample = 16;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			var iwf = 1 << (flagch + (flagbits << 1) + (flagsr << 2));
			uint capfilter = m_device.Caps.dwFormats;
			if ((capfilter & iwf) != 0) {
				suggestion = format;
				return true;
			}
			else {
				uint chfilter = 0x55555555U;
				if (flagch == 1) chfilter = ~chfilter;
				if ((capfilter & chfilter) != 0) capfilter &= chfilter;
				else capfilter &= ~chfilter;
				if (capfilter == 0) {
					suggestion = null;
					return false;
				}
				bool srmatchflag = false;
				for (byte iflagsr = flagsr; iflagsr < 8; iflagsr++) {
					uint srfilter = 0x0000000fU << (iflagsr << 2);
					if ((capfilter & srfilter) != 0) {
						capfilter &= srfilter;
						srmatchflag = true;
						break;
					}
				}
				if (!srmatchflag && flagsr > 0)
					for (byte iflagsr = (byte)(flagsr - 1); iflagsr >= 0; iflagsr--) {
						uint srfilter = 0x0000000fU << (iflagsr << 2);
						if ((capfilter & srfilter) != 0) {
							capfilter &= srfilter;
							srmatchflag = true;
							break;
						}
					}
				if (!srmatchflag) {
					suggestion = null;
					return false;
				}
				uint bitsfilter = 0x33333333U;
				if (flagbits == 1) bitsfilter = ~bitsfilter;
				if ((capfilter & flagbits) != 0) capfilter &= bitsfilter;
				else capfilter &= ~bitsfilter;
				if (capfilter == 0) {
					suggestion = null;
					return false;
				}
				uint sugsr;
				if      ((capfilter & 0x0000000f) != 0) sugsr = 11025;
				else if ((capfilter & 0x000000f0) != 0) sugsr = 22050;
				else if ((capfilter & 0x00000f00) != 0) sugsr = 44100;
				else if ((capfilter & 0x0000f000) != 0) sugsr = 48000;
				else if ((capfilter & 0x000f0000) != 0) sugsr = 96000;
				else throw new NotSupportedException("Theoretically unreachable");
				suggestion = new WaveFormat {
					Channels = (capfilter & 0x55555555) != 0 ? (ushort)1 : (ushort)2,
					SampleRate = sugsr,
					BitsPerSample = (capfilter & 0x33333333) != 0 ? (ushort)8 : (ushort)16,
				};
				return false;
			}
		}

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

		public override void Pause() {
			if (Playing) {
				MmSysComExports.MMR(MmeExports.waveOutPause(_waveOutHandle));
				/*MmSysComExports.MMR(MmeExports.waveOutReset(_waveOutHandle));
				m_bufferPosition = 0;*/
				_thread.Abort();
				_thread = null;
				base.Pause();
			}
		}

		Thread _thread;

		void ThreadLogic() {
			while (true) {
				for (int i = 0; i < BUFFER_COUNT; i++) {
					var b = _buffers[i];
					if ((b.Header.dwFlags & (uint)WHDR.DONE) != 0 || !b.Filled) {
						if (Source == null || Source.Muted) {
							Array.Clear(b.Buffer, 0, b.Buffer.Length);
						}
						else {
							Source.FillBuffer(b.Buffer, 0, BufferSize);
						}
						MmSysComExports.MMR(MmeExports.waveOutWrite(_waveOutHandle, ref b.Header, SIZE_WAVEHDR));
						b.Filled = true;
						m_bufferPosition += (double)BufferSize / m_format.BytesPerSecond;
					}
				}
				if (Synch.WaitForSingleObject(_eventHandle, 2000) != /* WAIT_OBJECT_0 */ 0)
					throw new InvalidOperationException("Error while pending for event");
			}
		}
	}
}
