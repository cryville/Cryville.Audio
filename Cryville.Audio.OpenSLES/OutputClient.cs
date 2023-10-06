using Cryville.Common.Interop;
using OpenSLES.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cryville.Audio.OpenSLES {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with OpenSL ES.
	/// </summary>
	public class OutputClient : AudioClient {
		static readonly List<OutputClient> _instances = new List<OutputClient>();
		readonly int _id;

		internal OutputClient(Engine engine, OutputDevice device, WaveFormat format, float bufferDuration, AudioShareMode shareMode) {
			_id = _instances.Count;
			_instances.Add(this);

			_objEngine = engine;
			m_device = device;

			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported.");
			if (bufferDuration == 0) bufferDuration = device.DefaultBufferDuration;
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

			Util.SLR(_bq.Obj.RegisterCallback(_bq, Callback, new IntPtr(_id)));
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
				_instances.Remove(this);
			}
		}

		readonly List<GCHandle> _handles = new List<GCHandle>();
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
		public override void Pause() {
			if (Playing) {
				lock (_enqlock) {
					Util.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PAUSED));
					base.Pause();
				}
			}
		}

		/// <inheritdoc />
		public override void Start() {
			if (!Playing) {
				Util.SLR(_bq.Obj.GetState(_bq, out var state));
				for (int i = 0; i < BUFFER_COUNT - state.count; i++) Enqueue();
				Util.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PLAYING));
				base.Start();
			}
		}

		readonly object _enqlock = new object();
		int _bufc;
		void Enqueue() {
			lock (_enqlock) {
				if (_bufc >= BUFFER_COUNT) return;
				_bufc++;
				if (Source == null || Muted) Array.Clear(_buf[_bufi], 0, BufferSize);
				else Source.Read(_buf[_bufi], 0, BufferSize);
				Util.SLR(_bq.Obj.Enqueue(_bq, _hbuf[_bufi++].AddrOfPinnedObject(), (uint)BufferSize));
				_bufi %= BUFFER_COUNT;
				m_bufferPosition += (double)BufferSize / Format.BytesPerSecond;
			}
		}

		[MonoPInvokeCallback]
		static void Callback(IntPtr caller, IntPtr pContext) {
			var i = _instances[pContext.ToInt32()];
			i._bufc--;
			i.Enqueue();
		}
	}
}
