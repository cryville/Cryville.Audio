using OpenSL.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cryville.Audio.OpenSL {
	/// <summary>
	/// An <see cref="AudioClient" /> that interacts with OpenSL ES.
	/// </summary>
	/// <remarks>
	/// See <see cref="CallbackFunction" /> if AOT is used.
	/// </remarks>
	public class OutputClient : AudioClient {
		readonly static List<OutputClient> _instances = new List<OutputClient>();
		readonly int _id;
		internal OutputClient(Engine engine, OutputDevice device) {
			_id = _instances.Count;
			_instances.Add(this);
			_objEngine = engine;
			m_device = device;
		}

		bool _disposed;
		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			if (!_disposed) {
				if (Playing) Pause();
				if (_objPlayer != null) _objPlayer.Obj.Destroy(_objPlayer);
				if (_objMix != null) _objMix.Obj.Destroy(_objMix);
				foreach (var h in _hbuf) h.Free();
				foreach (var h in _handles) h.Free();
				_instances.Remove(this);
				_disposed = true;
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
			BitsPerSample = 16,
		};

		WaveFormat m_format;
		/// <inheritdoc />
		public override WaveFormat Format => m_format;

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

		private static slBufferQueueCallback _cb = Callback;
		/// <summary>
		/// The buffer queue callback function.
		/// </summary>
		/// <remarks>
		/// <para>In the case where AOT is used, override this so it points to a proper function, which calls <see cref="Callback(IntPtr, IntPtr)" />, as the following code snippet:</para>
		/// <code>
		/// [MonoPInvokeCallback(typeof(slBufferQueueCallback))]
		/// static void AOTCallback(IntPtr caller, IntPtr context) {
		///	    OutputClient.Callback(caller, context);
		/// }
		/// </code>
		/// <para>You should not override this function in other cases.</para>
		/// </remarks>
		public static slBufferQueueCallback CallbackFunction { get => _cb; set => _cb = value; }

		const int BUFFER_COUNT = 2;
		int _bufi;
		readonly byte[][] _buf = new byte[BUFFER_COUNT][];
		readonly GCHandle[] _hbuf = new GCHandle[BUFFER_COUNT];

		/// <inheritdoc />
		public override void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			if (shareMode == AudioShareMode.Exclusive)
				throw new NotSupportedException("Exclusive mode not supported");
			if (bufferDuration == 0) bufferDuration = DefaultBufferDuration;
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

			Util.SLR(_bq.Obj.RegisterCallback(_bq, OutputClient.CallbackFunction, new IntPtr(_id)));
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
			switch (format.BitsPerSample) {
				case 8:
				case 16:
					suggestion = format;
					return true;
				default:
					if (format.BitsPerSample < 8) format.BitsPerSample = 8;
					else format.BitsPerSample = 16;
					suggestion = format;
					return false;
			}
		}

		/// <inheritdoc />
		public override void Pause() {
			if (Playing) {
				Util.SLR(_play.Obj.SetPlayState(_play, (UInt32)SL_PLAYSTATE.PAUSED));
				base.Pause();
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
				if (Source.Muted) Array.Clear(_buf[_bufi], 0, BufferSize);
				else Source.FillBuffer(_buf[_bufi], 0, BufferSize);
				Util.SLR(_bq.Obj.Enqueue(_bq, _hbuf[_bufi++].AddrOfPinnedObject(), (uint)BufferSize));
				_bufi %= BUFFER_COUNT;
				m_bufferPosition += (double)BufferSize / Format.BytesPerSecond;
			}
		}

		/// <summary>
		/// See <see cref="CallbackFunction" />.
		/// </summary>
		public static void Callback(IntPtr caller, IntPtr pContext) {
			var i = _instances[pContext.ToInt32()];
			i._bufc--;
			i.Enqueue();
		}
	}
}
