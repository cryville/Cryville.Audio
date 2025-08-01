using Cryville.Audio.OpenSLES.Native;
using Cryville.Interop.Mono;
using System;
using System.Collections.Generic;
using System.Globalization;
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
			Guid IID_SLAndroidConfigurationItf = typeof(SLAndroidConfigurationItf).GUID; ids[1] = new(&IID_SLAndroidConfigurationItf); req[1] = false;
			Helpers.SLR(_engine.Obj.CreateAudioPlayer(_engine, out var pObjPlayer, ref src, ref snk, 2, ids, req), "ObjEngine.CreateAudioPlayer");
			_objPlayer = new SLItfWrapper<SLObjectItf>(pObjPlayer);

			var getConfigResult = _objPlayer.Obj.GetInterface(_objPlayer, typeof(SLAndroidConfigurationItf).GUID, out var pConfig);
			if (getConfigResult == SLResult.SUCCESS) {
				var config = new SLItfWrapper<SLAndroidConfigurationItf>(pConfig);
				var streamType = Helpers.ToInternalStreamType(usage);
				Helpers.SLR(config.Obj.SetConfiguration(config, "androidPlaybackStreamType", new(&streamType), sizeof(SL_ANDROID_STREAM)), "ObjAndroidConfiguration.SetConfiguration");
			}
			else if (getConfigResult != SLResult.FEATURE_UNSUPPORTED) {
				Helpers.SLR(getConfigResult, "ObjPlayer.GetInterface(AndroidConfiguration)");
			}

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
		SLItfWrapper<SLPlayItf>? _play;

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
		public override AudioClientStatus Status {
			get {
				var play = _play;
				if (play == null) return AudioClientStatus.Closed;
				Helpers.SLR(play.Obj.GetPlayState(play, out var state));
				return (SL_PLAYSTATE)state switch {
					SL_PLAYSTATE.STOPPED or SL_PLAYSTATE.PAUSED => AudioClientStatus.Idle,
					SL_PLAYSTATE.PLAYING => AudioClientStatus.Playing,
					_ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unknown OpenSL ES state: {0}.", state)),
				};
			}
		}

		/// <inheritdoc />
		public override double Position {
			get {
				var play = _play ?? throw new ObjectDisposedException(null);
				Helpers.SLR(play.Obj.GetPosition(play, out uint mSec));
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
		public override void RequestStart() {
			var play = _play ?? throw new ObjectDisposedException(null);
			Helpers.SLR(_bq.Obj.GetState(_bq, out var state));
			for (int i = 0; i < BUFFER_COUNT - state.count; i++) Enqueue();
			Helpers.SLR(play.Obj.SetPlayState(play, (UInt32)SL_PLAYSTATE.PLAYING));
		}

		/// <inheritdoc />
		public override void RequestPause() {
			var play = _play ?? throw new ObjectDisposedException(null);
			lock (_enqLock) {
				Helpers.SLR(play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PAUSED));
			}
		}

		/// <inheritdoc />
		public override void Close() {
			var play = Interlocked.Exchange(ref _play, null);
			if (play == null) return;
			_objPlayer?.Obj.Destroy(_objPlayer);
			_objMix?.Obj.Destroy(_objMix);
			foreach (var h in _hBuf) h.Free();
			foreach (var h in _handles) h.Free();
			_instances.Remove(this);
		}

		readonly object _enqLock = new();
		int _freeBufferCount;
		void Enqueue() {
			lock (_enqLock) {
				if (_freeBufferCount >= BUFFER_COUNT) return;
				_freeBufferCount++;
				int length = BufferSize * m_format.FrameSize;
				if (Stream == null) AudioStream.SilentBuffer(Format, ref _buf[_bufIndex][0], BufferSize);
				else Stream.ReadFramesGreedily(_buf[_bufIndex], 0, BufferSize);
				Helpers.SLR(_bq.Obj.Enqueue(_bq, _hBuf[_bufIndex++].AddrOfPinnedObject(), (uint)length));
				_bufIndex %= BUFFER_COUNT;
				m_bufferPosition += (double)BufferSize / m_format.SampleRate;
			}
		}

		[MonoPInvokeCallback(typeof(slBufferQueueCallback))]
		static void Callback(IntPtr caller, IntPtr pContext) {
			var i = _instances[pContext.ToInt32()];
			i._freeBufferCount--;
			i.Enqueue();
		}
	}
}
