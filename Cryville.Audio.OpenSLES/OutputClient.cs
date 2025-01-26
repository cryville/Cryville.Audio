using Cryville.Audio.OpenSLES.Native;
using Cryville.Interop.Mono;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.OpenSLES {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with OpenSL ES.
	/// </summary>
	public class OutputClient : AudioClient {
		static readonly List<OutputClient> _instances = [];
		readonly int _id;

		internal unsafe OutputClient(Engine engine, OutputDevice device, WaveFormat format, int bufferSize, AudioUsage usage, AudioShareMode shareMode) {
			_id = _instances.Count;
			_instances.Add(this);

			_objEngine = engine;
			m_device = device;

			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported.");
			if (bufferSize == 0) bufferSize = device.DefaultBufferSize;
			m_format = format;

			IntPtr[] ids = new IntPtr[2];
			bool[] req = new bool[2];

			Helpers.SLR(_objEngine.ObjEngine.Obj.GetInterface(_objEngine.ObjEngine, typeof(SLEngineItf).GUID, out var pEngine), "ObjEngine.GetInterface");
			_engine = new SLItfWrapper<SLEngineItf>(pEngine);
			Helpers.SLR(_engine.Obj.CreateOutputMix(_engine, out var pObjMix, 0, ids, req), "ObjEngine.CreateOutputMix");
			_objMix = new SLItfWrapper<SLObjectItf>(pObjMix);
			Helpers.SLR(_objMix.Obj.Realize(_objMix, false), "ObjMix.Realize");

			var srcLoc = new SLDataLocator_BufferQueue(2);
			var hSrcLoc = GCHandle.Alloc(srcLoc, GCHandleType.Pinned); _handles.Add(hSrcLoc);
			var srcFmt = Helpers.ToInternalFormat(format);
			var hSrcFmt = GCHandle.Alloc(srcFmt, GCHandleType.Pinned); _handles.Add(hSrcFmt);
			var src = new SLDataSource(hSrcLoc.AddrOfPinnedObject(), hSrcFmt.AddrOfPinnedObject());
			var snkLoc = new SLDataLocator_OutputMix(_objMix);
			var hSnkLoc = GCHandle.Alloc(snkLoc, GCHandleType.Pinned); _handles.Add(hSnkLoc);
			var snk = new SLDataSink(hSnkLoc.AddrOfPinnedObject(), IntPtr.Zero);
			Guid IID_SLBufferQueueItf = typeof(SLBufferQueueItf).GUID; ids[0] = new(&IID_SLBufferQueueItf); req[0] = true;
			Helpers.SLR(_engine.Obj.CreateAudioPlayer(_engine, out var pObjPlayer, ref src, ref snk, 1, ref ids, req), "ObjEngine.CreateAudioPlayer");
			Helpers.SLR(_engine.Obj.CreateAudioPlayer(_engine, out var pObjPlayer, ref src, ref snk, 1, ids, req), "ObjEngine.CreateAudioPlayer");
			_objPlayer = new SLItfWrapper<SLObjectItf>(pObjPlayer);
			Helpers.SLR(_objPlayer.Obj.Realize(_objPlayer, false), "ObjPlayer.Realize");

			Helpers.SLR(_objPlayer.Obj.GetInterface(_objPlayer, typeof(SLBufferQueueItf).GUID, out var pbq), "ObjPlayer.GetInterface(BufferQueue)");
			_bq = new SLItfWrapper<SLBufferQueueItf>(pbq);
			Helpers.SLR(_objPlayer.Obj.GetInterface(_objPlayer, typeof(SLPlayItf).GUID, out var pPlay), "ObjPlayer.GetInterface(Play)");
			_play = new SLItfWrapper<SLPlayItf>(pPlay);

			m_bufferSize = bufferSize;
			for (int i = 0; i < BUFFER_COUNT; i++) {
				_buf[i] = new byte[m_bufferSize * m_format.FrameSize];
				_hBuf[i] = GCHandle.Alloc(_buf[i], GCHandleType.Pinned);
			}

			Helpers.SLR(_bq.Obj.RegisterCallback(_bq, Callback, new IntPtr(_id)), "ObjBufferQueue.RegisterCallback");
		}

		readonly List<GCHandle> _handles = [];
		readonly Engine _objEngine;
		readonly SLItfWrapper<SLEngineItf> _engine;
		readonly SLItfWrapper<SLObjectItf> _objMix;
		readonly SLItfWrapper<SLObjectItf> _objPlayer;
		readonly SLItfWrapper<SLBufferQueueItf> _bq;
		readonly SLItfWrapper<SLPlayItf> _play;

		readonly OutputDevice m_device;
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

		/// <inheritdoc />
		public override double Position {
			get {
				Helpers.SLR(_play.Obj.GetPosition(_play, out uint mSec));
				return mSec / 1000d;
			}
		}

		double m_bufferPosition;
		/// <inheritdoc />
		public override double BufferPosition => m_bufferPosition;

		const int BUFFER_COUNT = 2;
		int _bufIndex;
		readonly byte[][] _buf = new byte[BUFFER_COUNT][];
		readonly GCHandle[] _hBuf = new GCHandle[BUFFER_COUNT];

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
			Helpers.SLR(_bq.Obj.GetState(_bq, out var state));
			for (int i = 0; i < BUFFER_COUNT - state.count; i++) Enqueue();
			Helpers.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PLAYING));
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
			lock (_enqLock) {
				Helpers.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PAUSED));
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
			_objPlayer?.Obj.Destroy(_objPlayer);
			_objMix?.Obj.Destroy(_objMix);
			foreach (var h in _hBuf) h.Free();
			foreach (var h in _handles) h.Free();
			_instances.Remove(this);
			lock (_statusLock) m_status = AudioClientStatus.Closed;
		}

		readonly object _enqLock = new();
		int _freeBufferCount;
		void Enqueue() {
			lock (_enqLock) {
				if (_freeBufferCount >= BUFFER_COUNT) return;
				_freeBufferCount++;
				int length = BufferSize * m_format.FrameSize;
				if (Source == null || Muted) Array.Clear(_buf[_bufIndex], 0, length);
				else Source.ReadFrames(_buf[_bufIndex], 0, BufferSize);
				Helpers.SLR(_bq.Obj.Enqueue(_bq, _hBuf[_bufIndex++].AddrOfPinnedObject(), (uint)length));
				_bufIndex %= BUFFER_COUNT;
				m_bufferPosition += (double)BufferSize / m_format.SampleRate;
			}
		}

		delegate void DataHandler(IntPtr caller, IntPtr pContext);
		[MonoPInvokeCallback(typeof(DataHandler))]
		static void Callback(IntPtr caller, IntPtr pContext) {
			var i = _instances[pContext.ToInt32()];
			i._freeBufferCount--;
			i.Enqueue();
		}
	}
}
