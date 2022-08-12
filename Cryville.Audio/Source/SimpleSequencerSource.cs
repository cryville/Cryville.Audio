using Cryville.Common.Math;
using System;
using System.Collections.Generic;

namespace Cryville.Audio.Source {
	/// <summary>
	/// A simple <see cref="AudioSource" /> that mixes sequenced audio sources.
	/// </summary>
	/// <remarks>
	/// <para>To use this class, take the following steps:</para>
	/// <list type="number">
	/// <item>Create an instance of <see cref="SimpleSequencerSource" />.</item>
	/// <item>Attach the <see cref="SimpleSequencerSource" /> to an <see cref="AudioClient" /> by setting <see cref="AudioClient.Source" />.</item>
	/// <item>Create a new <see cref="SimpleSequencerSession" /> by calling <see cref="NewSession" />.</item>
	/// <item>Start playback by calling <see cref="AudioClient.Start" /> and setting <see cref="Playing" /> to <see langword="true" />.</item>
	/// </list>
	/// <para>You can sequence <see cref="AudioSource" />s to the <see cref="SimpleSequencerSession" /> both before and after playback starts. See <see cref="SimpleSequencerSession.Sequence" />.</para>
	/// <para>If <see cref="Playing" /> is set to <see langword="false" />, the output will become silence.</para>
	/// </remarks>
	public class SimpleSequencerSource : AudioSource {
		/// <summary>
		/// Creates an instance of the <see cref="SimpleSequencerSource" /> class.
		/// </summary>
		/// <param name="maxPolyphony">Max polyphony of the source. Must be greater than 0. See <see cref="MaxPolyphony"/>.</param>
		public SimpleSequencerSource(int maxPolyphony = 100) {
			if (maxPolyphony < 1) throw new ArgumentOutOfRangeException(nameof(maxPolyphony));
			MaxPolyphony = maxPolyphony;
			_sources = new List<AudioSource>(maxPolyphony);
			_rmsources = new List<AudioSource>(maxPolyphony);
		}

		protected override void Dispose(bool disposing) {
			Playing = false;
		}

		public override bool EndOfData => false;

		protected override void OnSetFormat() {
			base.OnSetFormat();
			_pribuf = new double[BufferSize / (Format.BitsPerSample / 8)];
			_secbuf = new byte[BufferSize];
			if (BufferSize == 0) Playing = false;
		}

		protected internal override bool IsFormatSupported(WaveFormat format) {
			return format.BitsPerSample == 8
				|| format.BitsPerSample == 16
				|| format.BitsPerSample == 32;
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
					throw new InvalidOperationException("No session is attached");
				m_playing = value;
				if (!value) {
					_sources.Clear();
				}
			}
		}
		double _time;
		readonly List<AudioSource> _sources;
		readonly List<AudioSource> _rmsources;
		double[] _pribuf;
		byte[] _secbuf;
		protected internal override unsafe void FillBuffer(byte[] buffer, int offset, int length) {
			if (m_playing) {
				switch (Format.BitsPerSample) {
					case 8: Array.Clear(_pribuf, 0, length); break;
					case 16: Array.Clear(_pribuf, 0, length / sizeof(short)); break;
					case 32: Array.Clear(_pribuf, 0, length / sizeof(int)); break;
				}
				_rmsources.Clear();
				foreach (var source in _sources) {
					FillBufferInternal(source, 0, length);
					if (source.EndOfData) _rmsources.Add(source);
				}
				foreach (var source in _rmsources)
					_sources.Remove(source);
				var seq = Session._seq;
				_time += (double)length / Format.BytesPerSecond;
				while (seq.Count > 0 && seq[0].Time < _time) {
					var item = seq[0];
					seq.RemoveAt(0);
					if (_sources.Count >= MaxPolyphony) continue;
					var source = item.Source;
					_sources.Add(source);
					int len = Math.Min(length, Format.Align((_time - item.Time) * Format.BytesPerSecond));
					FillBufferInternal(source, length - len, len);
				}
				switch (Format.BitsPerSample) {
					case 8:
						for (int i = offset; i < length + offset; i++) {
							buffer[i] = ClampScale.ToByte(_pribuf[i]);
						}
						break;
					case 16:
						fixed (byte* rptr = buffer) {
							short* ptr = (short*)(rptr + offset);
							for (int i = 0; i < length / sizeof(short); i++, ptr++) {
								*ptr = ClampScale.ToInt16(_pribuf[i]);
							}
						}
						break;
					case 32:
						fixed (byte* rptr = buffer) {
							int* ptr = (int*)(rptr + offset);
							for (int i = 0; i < length / sizeof(int); i++, ptr++) {
								*ptr = ClampScale.ToInt32(_pribuf[i]);
							}
						}
						break;
				}
			}
			else {
				for (int i = 0; i < length; i++)
					buffer[i + offset] = Format.BitsPerSample == 8 ? (byte)0x80 : (byte)0x00;
			}
		}
		unsafe void FillBufferInternal(AudioSource source, int offset, int length) {
			source.FillBuffer(_secbuf, offset, length);
			switch (Format.BitsPerSample) {
				case 8:
					for (int i = offset; i < length; i++) {
						_pribuf[i] += _secbuf[i] / (double)0x80 - 1;
					}
					break;
				case 16:
					fixed (byte* rptr = _secbuf) {
						short* ptr = (short*)rptr;
						for (int i = offset / sizeof(short); i < length / sizeof(short); i++, ptr++) {
							_pribuf[i] += *ptr / (double)0x8000;
						}
					}
					break;
				case 32:
					fixed (byte* rptr = _secbuf) {
						int* ptr = (int*)rptr;
						for (int i = offset / sizeof(int); i < length / sizeof(int); i++, ptr++) {
							_pribuf[i] += *ptr / (double)0x80000000;
						}
					}
					break;
			}
		}

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
			if (BufferSize == 0) throw new InvalidOperationException("Audio source not attached to client");
			m_playing = false;
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
			public AudioSource Source { get; set; }
			public Schedule(double time, AudioSource source) {
				Time = time;
				Source = source;
			}
			public int CompareTo(Schedule other) {
				return Time.CompareTo(other.Time);
			}
		}
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
		/// <param name="time">The time.</param>
		/// <param name="source">The audio source.</param>
		/// <remarks>
		/// <para>If <paramref name="time" /> is less than the current time, the <paramref name="source" /> will be played immediately.</para>
		/// <para>If the number of audio sources currently playing exceeds <see cref="SimpleSequencerSource.MaxPolyphony" />, the <paramref name="source" /> will be discarded.</para>
		/// </remarks>
		public void Sequence(double time, AudioSource source) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			source.SetFormat(_format, _bufferSize);
			var sch = new Schedule(time, source);
			var index = _seq.BinarySearch(sch);
			if (index < 0) index = ~index;
			_seq.Insert(index, sch);
		}
	}
}
