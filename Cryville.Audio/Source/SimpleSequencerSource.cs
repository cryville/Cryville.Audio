using Cryville.Common.Math;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cryville.Audio.Source {
	/// <summary>
	/// A simple <see cref="AudioStream" /> that mixes sequenced audio sources.
	/// </summary>
	/// <remarks>
	/// <para>To use this class, take the following steps:</para>
	/// <list type="number">
	/// <item>Create an instance of <see cref="SimpleSequencerSource" />.</item>
	/// <item>Attach the <see cref="SimpleSequencerSource" /> to an <see cref="AudioClient" /> by setting <see cref="AudioClient.Source" />.</item>
	/// <item>Create a new <see cref="SimpleSequencerSession" /> by calling <see cref="NewSession" />.</item>
	/// <item>Start playback by calling <see cref="AudioClient.Start" /> and setting <see cref="Playing" /> to <see langword="true" />.</item>
	/// </list>
	/// <para><see cref="AudioStream" />s can be sequenced to the <see cref="SimpleSequencerSession" /> both before and after playback starts. See <see cref="SimpleSequencerSession.Sequence" />.</para>
	/// <para>If <see cref="Playing" /> is set to <see langword="false" />, the output will become silence.</para>
	/// </remarks>
	public class SimpleSequencerSource : AudioStream {
		/// <summary>
		/// Creates an instance of the <see cref="SimpleSequencerSource" /> class.
		/// </summary>
		/// <param name="maxPolyphony">Max polyphony of the source. Must be greater than 0. See <see cref="MaxPolyphony"/>.</param>
		public SimpleSequencerSource(int maxPolyphony = 100) {
			if (maxPolyphony < 1) throw new ArgumentOutOfRangeException(nameof(maxPolyphony));
			MaxPolyphony = maxPolyphony;
			_sources = new List<AudioStream>(maxPolyphony);
			_rmsources = new List<AudioStream>(maxPolyphony);
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
		public override bool EndOfData => false;

		/// <inheritdoc />
		protected override void OnSetFormat() {
			_pribuf = new double[BufferSize / (Format.BitsPerSample / 8)];
			_secbuf = new byte[BufferSize];
			if (BufferSize == 0) Playing = false;
		}

		/// <inheritdoc />
		protected internal override bool IsFormatSupported(WaveFormat format) {
			return format.SampleFormat == SampleFormat.U8
				|| format.SampleFormat == SampleFormat.S16
				|| format.SampleFormat == SampleFormat.S32
				|| format.SampleFormat == SampleFormat.F32
				|| format.SampleFormat == SampleFormat.F64;
		}

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
		long _pos;
		double _time;
		readonly object _lock = new object();
		readonly List<AudioStream> _sources;
		readonly List<AudioStream> _rmsources;
		double[] _pribuf;
		byte[] _secbuf;
		/// <inheritdoc />
		public override unsafe int Read(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Disposed) throw new ObjectDisposedException(null);
			count = Format.Align(count, true);
			if (m_playing) {
				Array.Clear(_pribuf, 0, count / (Format.BitsPerSample / 8));
				lock (_lock) {
					_rmsources.Clear();
					foreach (var source in _sources) {
						FillBufferInternal(source, 0, count);
						if (source.EndOfData) _rmsources.Add(source);
					}
					foreach (var source in _rmsources)
						_sources.Remove(source);
					lock (Session._lock) {
						var seq = Session._seq;
						_time += (double)count / Format.BytesPerSecond;
						while (seq.Count > 0 && seq[0].Time < _time) {
							var item = seq[0];
							seq.RemoveAt(0);
							if (_sources.Count >= MaxPolyphony) continue;
							var source = item.Source;
							_sources.Add(source);
							int len = Math.Min(count, Format.Align((_time - item.Time) * Format.BytesPerSecond, true));
							FillBufferInternal(source, count - len, len);
						}
					}
				}
				switch (Format.SampleFormat) {
					case SampleFormat.U8:
						for (int i = offset; i < count + offset; i++) {
							buffer[i] = ClampScale.ToByte(_pribuf[i]);
						}
						break;
					case SampleFormat.S16:
						fixed (byte* rptr = buffer) {
							short* ptr = (short*)(rptr + offset);
							for (int i = 0; i < count / sizeof(short); i++, ptr++) {
								*ptr = ClampScale.ToInt16(_pribuf[i]);
							}
						}
						break;
					case SampleFormat.S32:
						fixed (byte* rptr = buffer) {
							int* ptr = (int*)(rptr + offset);
							for (int i = 0; i < count / sizeof(int); i++, ptr++) {
								*ptr = ClampScale.ToInt32(_pribuf[i]);
							}
						}
						break;
					case SampleFormat.F32:
						fixed (byte* rptr = buffer) {
							float* ptr = (float*)(rptr + offset);
							for (int i = 0; i < count / sizeof(float); i++, ptr++) {
								*ptr = (float)_pribuf[i];
							}
						}
						break;
					case SampleFormat.F64:
						fixed (byte* rptr = buffer) {
							double* ptr = (double*)(rptr + offset);
							for (int i = 0; i < count / sizeof(double); i++, ptr++) {
								*ptr = _pribuf[i];
							}
						}
						break;
				}
				_pos += count;
				return count;
			}
			else {
				SilentBuffer(Format, buffer, offset, count);
				return 0;
			}
		}
		unsafe void FillBufferInternal(AudioStream source, int offset, int count) {
			count = source.Read(_secbuf, offset, count);
			switch (Format.SampleFormat) {
				case SampleFormat.U8:
					for (int i = offset; i < count; i++) {
						_pribuf[i] += _secbuf[i] / (double)0x80 - 1;
					}
					break;
				case SampleFormat.S16:
					fixed (byte* rptr = _secbuf) {
						short* ptr = (short*)rptr;
						for (int i = offset / sizeof(short); i < count / sizeof(short); i++, ptr++) {
							_pribuf[i] += *ptr / (double)0x8000;
						}
					}
					break;
				case SampleFormat.S32:
					fixed (byte* rptr = _secbuf) {
						int* ptr = (int*)rptr;
						for (int i = offset / sizeof(int); i < count / sizeof(int); i++, ptr++) {
							_pribuf[i] += *ptr / (double)0x80000000;
						}
					}
					break;
				case SampleFormat.F32:
					fixed (byte* rptr = _secbuf) {
						float* ptr = (float*)rptr;
						for (int i = offset / sizeof(float); i < count / sizeof(float); i++, ptr++) {
							_pribuf[i] += *ptr;
						}
					}
					break;
				case SampleFormat.F64:
					fixed (byte* rptr = _secbuf) {
						double* ptr = (double*)rptr;
						for (int i = offset / sizeof(double); i < count / sizeof(double); i++, ptr++) {
							_pribuf[i] += *ptr;
						}
					}
					break;
			}
		}

		/// <inheritdoc />
		/// <param name="offset">A byte offset relative to the current position.</param>
		/// <param name="origin">Must be <see cref="SeekOrigin.Current" />.</param>
		/// <remarks>
		/// <para>This stream can only be seeked from the current position, and forward only. Thus, <paramref name="offset" /> must be non-negative, and <paramref name="origin" /> must be <see cref="SeekOrigin.Current" />.</para>
		/// </remarks>
		public override long Seek(long offset, SeekOrigin origin) {
			if (origin != SeekOrigin.Current) throw new ArgumentException("Must seek from current position.", nameof(origin));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (Disposed) throw new ObjectDisposedException(null);
			offset = Format.Align(offset, true);
			lock (_lock) {
				_rmsources.Clear();
				foreach (var source in _sources) {
					if (!source.CanSeek) continue;
					source.Seek(offset, origin);
					if (source.EndOfData) _rmsources.Add(source);
				}
				foreach (var source in _rmsources)
					_sources.Remove(source);
				_time += (double)offset / Format.BytesPerSecond;
				lock (Session._lock) {
					var seq = Session._seq;
					while (seq.Count > 0 && seq[0].Time < _time) {
						var item = seq[0];
						seq.RemoveAt(0);
						var source = item.Source;
						if (!source.CanSeek) continue;
						var len = Format.Align((_time - item.Time) * Format.BytesPerSecond, true);
						source.Seek(len, origin);
						if (!source.EndOfData) _sources.Add(source);
					}
				}
			}
			_pos += offset;
			return _pos;
		}

		/// <inheritdoc />
		public override bool CanRead => true;
		/// <inheritdoc />
		/// <remarks>
		/// <para>This stream can only be seeked from the current position, and forward only. See <see cref="Seek(long, SeekOrigin)" />.</para>
		/// </remarks>
		public override bool CanSeek => true;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override long Length => long.MaxValue;
		/// <inheritdoc />
		/// <remarks>
		/// <para>Although this stream is seekable, setting this property is not supported and throws <see cref="NotSupportedException" />. This stream can only be seeked from the current position, and forward only. See <see cref="Seek(long, SeekOrigin)" />.</para>
		/// </remarks>
		public override long Position { get => _pos; set => throw new NotSupportedException(); }
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
		public SimpleSequencerSession Session { get; private set; }
		/// <summary>
		/// Stops the current session and creates a new <see cref="SimpleSequencerSession" /> to replace it.
		/// </summary>
		/// <remarks>
		/// An <see cref="AudioClient" /> must be attached to this source first.
		/// </remarks>
		public SimpleSequencerSession NewSession() {
			if (BufferSize == 0) throw new InvalidOperationException("Sequencer not attached to client.");
			if (Disposed) throw new ObjectDisposedException(null);
			m_playing = false;
			lock (_lock) _sources.Clear();
			_time = 0;
			return Session = new SimpleSequencerSession(Format, BufferSize);
		}
	}

	/// <summary>
	/// A session for <see cref="SimpleSequencerSource" />.
	/// </summary>
	public class SimpleSequencerSession {
		internal struct Schedule : IComparable<Schedule> {
			public double Time { get; set; }
			public AudioStream Source { get; set; }
			public Schedule(double time, AudioStream source) {
				Time = time;
				Source = source;
			}
			public int CompareTo(Schedule other) {
				return Time.CompareTo(other.Time);
			}
		}
		internal object _lock = new object();
		internal List<Schedule> _seq = new List<Schedule>();
		readonly WaveFormat _format;
		readonly int _bufferSize;
		internal SimpleSequencerSession(WaveFormat format, int bufferSize) {
			_format = format;
			_bufferSize = bufferSize;
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
			source.SetFormat(_format, _bufferSize);
			var sch = new Schedule(time, source);
			lock (_lock) {
				var index = _seq.BinarySearch(sch);
				if (index < 0) index = ~index;
				_seq.Insert(index, sch);
			}
		}
	}
}
