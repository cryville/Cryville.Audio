using System;
using System.Collections.Generic;
using System.IO;
using UnsafeIL;

namespace Cryville.Audio.Source {
	/// <summary>
	/// A simple <see cref="AudioStream" /> that mixes sequenced audio sources.
	/// </summary>
	/// <remarks>
	/// <para>To use this class, take the following steps:</para>
	/// <list type="number">
	/// <item>Create an instance of <see cref="SimpleSequencerSource" />.</item>
	/// <item>Attach the <see cref="SimpleSequencerSource" /> to an <see cref="AudioClient" /> by setting <see cref="AudioClient.Stream" />.</item>
	/// <item>Create a new <see cref="SimpleSequencerSession" /> by calling <see cref="NewSession" />.</item>
	/// <item>Start playback and setting <see cref="Playing" /> to <see langword="true" />.</item>
	/// </list>
	/// <para><see cref="AudioStream" />s can be sequenced to the <see cref="SimpleSequencerSession" /> both before and after playback starts. See <see cref="SimpleSequencerSession.Sequence" />.</para>
	/// <para>If <see cref="Playing" /> is set to <see langword="false" />, the output will become silence.</para>
	/// </remarks>
	public class SimpleSequencerSource : AudioDoubleSampleStream {
		/// <summary>
		/// Creates an instance of the <see cref="SimpleSequencerSource" /> class.
		/// </summary>
		/// <param name="format">The wave format.</param>
		/// <param name="maxPolyphony">Max polyphony of the source. Must be greater than 0. See <see cref="MaxPolyphony"/>.</param>
		public SimpleSequencerSource(WaveFormat format, int maxPolyphony = 100) : base(format) {
			if (maxPolyphony < 1) throw new ArgumentOutOfRangeException(nameof(maxPolyphony));
			MaxPolyphony = maxPolyphony;
			_sources = new List<AudioStream>(maxPolyphony);
			_rmsources = new List<AudioStream>(maxPolyphony);
			_sampleReader = SampleConvert.GetSampleReader(format.SampleFormat);
			_sampleWriter = SampleConvert.GetSampleWriter(format.SampleFormat);
		}

		/// <summary>
		/// Whether this audio stream has been disposed.
		/// </summary>
		public bool Disposed { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (disposing) {
				Playing = false;
			}
			Disposed = true;
		}

		/// <inheritdoc />
		protected override int EnsureBufferSize(int targetBufferSize) {
			int newBufferSize = base.EnsureBufferSize(targetBufferSize);
			if (newBufferSize <= BufferSize) return BufferSize;
			_secbuf = new byte[newBufferSize * Format.FrameSize];
			_secbufd = new double[newBufferSize * Format.Channels];
			return newBufferSize;
		}

		/// <inheritdoc />
		public override long FrameLength => long.MaxValue;

		readonly SampleReader _sampleReader;
		readonly SampleWriter _sampleWriter;

		bool m_playing;
		/// <summary>
		/// Whether if the current session is playing.
		/// </summary>
		/// <remarks>
		/// There is a tiny delay before the playback state actually toggles, which is approximately <see cref="AudioClient.BufferPosition" /> substracted by <see cref="AudioClient.Position" />.
		/// </remarks>
		public bool Playing {
			get { return m_playing; }
			set {
				if (Session == null && value)
					throw new InvalidOperationException("No session is active.");
				m_playing = value;
			}
		}
		readonly object _lock = new();
		readonly List<AudioStream> _sources;
		readonly List<AudioStream> _rmsources;
		byte[] _secbuf = [];
		double[] _secbufd = [];
		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref double buffer, int frameCount) {
			if (Disposed) throw new ObjectDisposedException(null);
			if (frameCount == 0) return 0;
			fixed (double* ptr = &buffer) {
				Unsafe.InitBlockUnaligned(ptr, 0, (uint)(frameCount * Format.Channels * sizeof(double)));
				if (!m_playing || Session == null) {
					return frameCount;
				}
				lock (_lock) {
					_rmsources.Clear();
					foreach (var source in _sources) {
						int readFrameCount = FillBufferInternal(source, ptr, 0, frameCount);
						if (readFrameCount < frameCount) _rmsources.Add(source);
					}
					foreach (var source in _rmsources)
						_sources.Remove(source);
					lock (Session._lock) {
						var seq = Session._seq;
						Session.FramePosition += frameCount;
						while (seq.Count > 0 && seq[0].Time < Session.TimePosition) {
							var item = seq[0];
							seq.RemoveAt(0);
							if (_sources.Count >= MaxPolyphony) continue;
							var source = item.Source;
							_sources.Add(source);
							int secFrameCount = Math.Min(frameCount, (int)((Session.TimePosition - item.Time) * Format.SampleRate));
							FillBufferInternal(source, ptr, frameCount - secFrameCount, secFrameCount);
						}
					}
				}
			}
			return frameCount;
		}
		unsafe int FillBufferInternal(AudioStream source, double* buffer, int frameOffset, int frameCount) {
			if (frameCount == 0) return 0;
			if (source is AudioDoubleSampleStream sourced) {
				frameCount = sourced.ReadFramesGreedily(ref _secbufd[0], frameCount);
				var sampleOffset = frameOffset * Format.Channels;
				var sampleCount = frameCount * Format.Channels;
				var pptr = buffer + sampleOffset;
				fixed (double* rptr = _secbufd) {
					double* ptr = rptr;
					for (int i = 0; i < sampleCount; i++) {
						*pptr++ += *ptr;
					}
				}
				return frameCount;
			}
			else {
				frameCount = source.ReadFramesGreedily(ref _secbuf[0], frameCount);
				var sampleOffset = frameOffset * Format.Channels;
				var sampleCount = frameCount * Format.Channels;
				var pptr = buffer + sampleOffset;
				fixed (byte* rptr = _secbuf) {
					byte* ptr = rptr;
					for (int i = 0; i < sampleCount; i++) {
						*pptr++ += _sampleReader(ref ptr);
					}
				}
				return frameCount;
			}
		}

		/// <inheritdoc />
		/// <param name="frameOffset">A byte offset relative to the current position.</param>
		/// <param name="origin">Must be <see cref="SeekOrigin.Current" />.</param>
		/// <remarks>
		/// <para>This stream can only be sought from the current position, and forward only. Thus, <paramref name="frameOffset" /> must be non-negative, and <paramref name="origin" /> must be <see cref="SeekOrigin.Current" />.</para>
		/// </remarks>
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin) {
			if (origin != SeekOrigin.Current) throw new ArgumentException("Must seek from current position.", nameof(origin));
			if (frameOffset < 0) throw new ArgumentOutOfRangeException(nameof(frameOffset));
			if (Disposed) throw new ObjectDisposedException(null);
			if (Session == null) throw new InvalidOperationException("No session is active.");
			lock (_lock) {
				_rmsources.Clear();
				foreach (var source in _sources) {
					if (!source.CanSeek) continue;
					long newFrameOffset = source.SeekFrame(frameOffset, origin);
					if (newFrameOffset >= source.FrameLength) _rmsources.Add(source);
				}
				foreach (var source in _rmsources)
					_sources.Remove(source);
				Session.FramePosition += frameOffset;
				lock (Session._lock) {
					var seq = Session._seq;
					while (seq.Count > 0 && seq[0].Time < Session.TimePosition) {
						var item = seq[0];
						seq.RemoveAt(0);
						var source = item.Source;
						if (!source.CanSeek) continue;
						long frameCount = (long)((Session.TimePosition - item.Time) * Format.SampleRate);
						long newFrameOffset = source.SeekFrame(frameCount, origin);
						if (newFrameOffset < source.FrameLength) _sources.Add(source);
					}
				}
			}
			return FramePosition + frameOffset;
		}

		/// <inheritdoc />
		public override bool CanRead => true;
		/// <inheritdoc />
		/// <remarks>
		/// <para>This stream can only be sought from the current position, and forward only. See <see cref="SeekFrameInternal(long, SeekOrigin)" />.</para>
		/// </remarks>
		public override bool CanSeek => true;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		/// <summary>
		/// The number of sources currently playing.
		/// </summary>
		public int Polyphony => _sources.Count;
		/// <summary>
		/// Max polyphony, the number of sources that can be played at the same time.
		/// </summary>
		public int MaxPolyphony { get; private set; }
		/// <summary>
		/// The <see cref="SimpleSequencerSession" /> currently playing.
		/// </summary>
		public SimpleSequencerSession? Session { get; private set; }
		/// <summary>
		/// Stops the current session and creates a new <see cref="SimpleSequencerSession" /> to replace it.
		/// </summary>
		/// <remarks>
		/// An <see cref="AudioClient" /> must be attached to this source first.
		/// </remarks>
		public SimpleSequencerSession NewSession() {
			if (Disposed) throw new ObjectDisposedException(null);
			m_playing = false;
			lock (_lock) _sources.Clear();
			return Session = new SimpleSequencerSession(Format);
		}
	}

	/// <summary>
	/// A session for <see cref="SimpleSequencerSource" />.
	/// </summary>
	public class SimpleSequencerSession {
		internal struct Schedule(double time, AudioStream source) : IComparable<Schedule> {
			public double Time { get; set; } = time;
			public AudioStream Source { get; set; } = source;

			public readonly int CompareTo(Schedule other) => Time.CompareTo(other.Time);
		}
		internal object _lock = new();
		internal List<Schedule> _seq = [];
		readonly WaveFormat _format;
		/// <summary>
		/// The time in frames within the current session.
		/// </summary>
		public long FramePosition { get; internal set; }
		/// <summary>
		/// The time in seconds within the current session.
		/// </summary>
		public double TimePosition => (double)FramePosition / _format.SampleRate;
		internal SimpleSequencerSession(WaveFormat format) {
			_format = format;
		}
		/// <summary>
		/// Sequences a <paramref name="source" /> at the specified <paramref name="time" />.
		/// </summary>
		/// <param name="time">The time in seconds.</param>
		/// <param name="source">The audio source.</param>
		/// <remarks>
		/// <para>If <paramref name="time" /> is less than the current time, the <paramref name="source" /> will be played immediately.</para>
		/// <para>If the number of audio sources currently playing exceeds <see cref="SimpleSequencerSource.MaxPolyphony" />, the <paramref name="source" /> will be discarded.</para>
		/// <para>Audio sources can be sequenced even when the sequencer has been disposed, while it would not have any effect.</para>
		/// </remarks>
		public void Sequence(double time, AudioStream source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			var sch = new Schedule(time, source);
			lock (_lock) {
				var index = _seq.BinarySearch(sch);
				if (index < 0) index = ~index;
				_seq.Insert(index, sch);
			}
		}
	}

	/// <summary>
	/// A builder that builds <see cref="SimpleSequencerSource" />.
	/// </summary>
	public class SimpleSequencerSourceBuilder : AudioStreamBuilder<SimpleSequencerSource> {
		/// <inheritdoc />
		public override WaveFormat DefaultFormat => WaveFormat.Default;
		/// <inheritdoc />
		public override bool IsFormatSupported(WaveFormat format) => true;

		/// <summary>
		/// Max polyphony, the number of sources that can be played at the same time.
		/// </summary>
		public int MaxPolyphony { get; set; }

		/// <inheritdoc />
		public override SimpleSequencerSource Build(WaveFormat format) => new(format, MaxPolyphony);
	}
}
