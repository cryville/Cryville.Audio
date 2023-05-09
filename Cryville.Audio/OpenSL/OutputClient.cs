using OpenSL.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.OpenSL {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with OpenSL ES.
	/// </summary>
	public class OutputClient : AudioClient {
		internal OutputClient(Engine engine, OutputDevice device) {
			_objEngine = engine;
			m_device = device;
		}

		/// <inheritdoc />
		~OutputClient() {
			Dispose(false);
		}

		int _disposeCount;
		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			if (Interlocked.Increment(ref _disposeCount) == 1) {
				if (Playing) Pause();
				_objPlayer?.Obj.Destroy(_objPlayer);
				_objMix?.Obj.Destroy(_objMix);
				foreach (var h in _hbuf) h.Free();
				foreach (var h in _handles) h.Free();
			}
		}

		readonly List<GCHandle> _handles = new List<GCHandle>();
		readonly Engine _objEngine;
		SLItfWrapper<SLEngineItf> _engine;
		SLItfWrapper<SLObjectItf> _objMix;
		SLItfWrapper<SLObjectItf> _objPlayer;
		SLItfWrapper<SLBufferQueueItf> _bq;
		SLItfWrapper<SLPlayItf> _play;

		readonly OutputDevice m_device;
		/// <inheritdoc />
		public override IAudioDevice Device => m_device;

		/// <inheritdoc />
		public override float DefaultBufferDuration => 20;

		/// <inheritdoc />
		public override float MinimumBufferDuration => 0;

		/// <inheritdoc />
		public override WaveFormat DefaultFormat => new WaveFormat {
			Channels = 2,
			SampleRate = 48000,
			SampleFormat = SampleFormat.S16,
		};

		WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format => m_format;

		int _bufferDuration;
		int m_bufferSize;
		/// <inheritdoc />
		public override int BufferSize => m_bufferSize;

		/// <inheritdoc />
		public override float MaximumLatency => 0;

		/// <inheritdoc />
		public override double Position {
			get {
				Util.SLR(_play.Obj.GetPosition(_play, out uint msec));
				return msec / 1000d;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		const int BUFFER_COUNT = 2;
		int _bufi;
		readonly byte[][] _buf = new byte[BUFFER_COUNT][];
		readonly GCHandle[] _hbuf = new GCHandle[BUFFER_COUNT];

		/// <inheritdoc />
		public override void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported.");
			if (bufferDuration == 0) bufferDuration = DefaultBufferDuration;
			_bufferDuration = (int)bufferDuration;
			m_format = format;

			Guid[] ids = new Guid[4];
			bool[] req = new bool[4];

			Util.SLR(_objEngine.ObjEngine.Obj.GetInterface(_objEngine.ObjEngine, typeof(SLEngineItf).GUID, out var pEngine));
			_engine = new SLItfWrapper<SLEngineItf>(pEngine);
			Util.SLR(_engine.Obj.CreateOutputMix(_engine, out var pObjMix, 0, ref ids, req));
			_objMix = new SLItfWrapper<SLObjectItf>(pObjMix);
			Util.SLR(_objMix.Obj.Realize(_objMix, false));

			var srcloc = new SLDataLocator_BufferQueue(2);
			var hsrcloc = GCHandle.Alloc(srcloc, GCHandleType.Pinned); _handles.Add(hsrcloc);
			var srcfmt = Util.ToInternalFormat(format);
			var hsrcfmt = GCHandle.Alloc(srcfmt, GCHandleType.Pinned); _handles.Add(hsrcfmt);
			var src = new SLDataSource(hsrcloc.AddrOfPinnedObject(), hsrcfmt.AddrOfPinnedObject());
			var snkloc = new SLDataLocator_OutputMix(_objMix);
			var hsnkloc = GCHandle.Alloc(snkloc, GCHandleType.Pinned); _handles.Add(hsnkloc);
			var snk = new SLDataSink(hsnkloc.AddrOfPinnedObject(), IntPtr.Zero);
			ids[0] = typeof(SLBufferQueueItf).GUID; req[0] = true;
			Util.SLR(_engine.Obj.CreateAudioPlayer(_engine, out var pObjPlayer, ref src, ref snk, 1, ref ids, req));
			_objPlayer = new SLItfWrapper<SLObjectItf>(pObjPlayer);
			Util.SLR(_objPlayer.Obj.Realize(_objPlayer, false));

			Util.SLR(_objPlayer.Obj.GetInterface(_objPlayer, typeof(SLBufferQueueItf).GUID, out var pbq));
			_bq = new SLItfWrapper<SLBufferQueueItf>(pbq);
			Util.SLR(_objPlayer.Obj.GetInterface(_objPlayer, typeof(SLPlayItf).GUID, out var pPlay));
			_play = new SLItfWrapper<SLPlayItf>(pPlay);

			m_bufferSize = format.Align(bufferDuration / 1000 * format.BytesPerSecond);
			for (int i = 0; i < BUFFER_COUNT; i++) {
				_buf[i] = new byte[m_bufferSize];
				_hbuf[i] = GCHandle.Alloc(_buf[i], GCHandleType.Pinned);
			}
		}

		/// <inheritdoc />
		public override bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			switch (format.Channels) {
				case 1:
				case 2:
					break;
				default:
					format.Channels = 2;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			switch (format.SampleRate) {
				case 8000:
				case 11025:
				case 12000:
				case 16000:
				case 22050:
				case 24000:
				case 32000:
				case 44100:
				case 48000:
					break;
				default:
					if (format.SampleRate < 8000) format.SampleRate = 8000;
					else if (format.SampleRate < 11025) format.SampleRate = 11025;
					else if (format.SampleRate < 12000) format.SampleRate = 12000;
					else if (format.SampleRate < 16000) format.SampleRate = 16000;
					else if (format.SampleRate < 22050) format.SampleRate = 22050;
					else if (format.SampleRate < 24000) format.SampleRate = 24000;
					else if (format.SampleRate < 32000) format.SampleRate = 32000;
					else if (format.SampleRate < 44100) format.SampleRate = 44100;
					else format.SampleRate = 48000;
					IsFormatSupported(format, out suggestion, shareMode);
					return false;
			}
			switch (format.SampleFormat) {
				case SampleFormat.U8:
				case SampleFormat.S16:
					suggestion = format;
					return true;
				default:
					format.SampleFormat = SampleFormat.S16;
					suggestion = format;
					return false;
			}
		}

		/// <inheritdoc />
		public override void Start() {
			if (!Playing) {
				Util.SLR(_bq.Obj.GetState(_bq, out var state));
				for (int i = (int)state.count; i < BUFFER_COUNT; i++) Enqueue();
				_thread = new Thread(new ThreadStart(ThreadLogic)) {
					Priority = ThreadPriority.Highest,
					IsBackground = true,
				};
				_thread.Start();
				Util.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PLAYING));
				base.Start();
			}
		}

		/// <inheritdoc />
		public override void Pause() {
			if (Playing) {
				lock (_enqlock) {
					_threadAbortFlag = true;
					if (!_thread.Join(1000))
						throw new InvalidOperationException("Failed to pause output client.");
					_thread = null;
					Util.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PAUSED));
					base.Pause();
				}
			}
		}

		Thread _thread;
		bool _threadAbortFlag;
		readonly object _enqlock = new object();
		void ThreadLogic() {
			_threadAbortFlag = false;
			Stopwatch timer = new Stopwatch();
			while (true) {
				int pendingBufferCount;
				while (true) {
					Util.SLR(_bq.Obj.GetState(_bq, out var state));
					if (state.count < BUFFER_COUNT) {
						pendingBufferCount = (int)state.count;
						break;
					}
					Thread.Sleep(1);
					if (_threadAbortFlag) return;
				}
				int timeout = _bufferDuration;
				timer.Reset();
				timer.Start();
				for (int i = pendingBufferCount; i < BUFFER_COUNT; i++) Enqueue();
				timeout -= (int)timer.ElapsedMilliseconds + 1;
				if (timeout < 0) timeout = 0;
				Thread.Sleep(timeout);
				if (_threadAbortFlag) return;
			}
		}
		void Enqueue() {
			lock (_enqlock) {
				if (Source == null || Muted) Array.Clear(_buf[_bufi], 0, BufferSize);
				else Source.Read(_buf[_bufi], 0, BufferSize);
				Util.SLR(_bq.Obj.Enqueue(_bq, _hbuf[_bufi++].AddrOfPinnedObject(), (uint)BufferSize));
				_bufi %= BUFFER_COUNT;
				m_bufferPosition += (double)BufferSize / Format.BytesPerSecond;
			}
		}
	}
}
