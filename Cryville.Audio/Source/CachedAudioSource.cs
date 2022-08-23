using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioSource" /> that caches data for reuse.
	/// </summary>
	public class CachedAudioSource : AudioSource {
		/// <summary>
		/// Creates an instance of the <see cref="CachedAudioSource" /> class.
		/// </summary>
		/// <param name="source">The <see cref="AudioSource" /> to be cached.</param>
		/// <param name="duration">The duration of the cache.</param>
		public CachedAudioSource(AudioSource source, double duration) {
			_cache = new Cache(source, duration);
		}

		/// <summary>
		/// Gets a clone of this <see cref="CachedAudioSource" /> with the timestamp reset.
		/// </summary>
		/// <returns>A clone of this <see cref="CachedAudioSource" /> with the timestamp reset.</returns>
		/// <remarks>
		/// Use with object pool is recommended.
		/// </remarks>
		public CachedAudioSource Clone() {
			var result = (CachedAudioSource)MemberwiseClone();
			result.Rewind();
			return result;
		}

		/// <summary>
		/// Resets the timestamp to reuse the instance.
		/// </summary>
		/// <remarks>
		/// Use with object pool is recommended.
		/// </remarks>
		public void Rewind() { _pos = 0; }

		class Cache {
			internal int LoadPosition;
			internal readonly AudioSource Source;
			internal readonly double Duration;
			internal byte[] Buffer;
			public Cache(AudioSource source, double duration) {
				Source = source;
				Duration = duration;
			}
			public void SetFormat(WaveFormat format, int bufferSize) {
				Source.SetFormat(format, bufferSize);
				int len = format.Align(Duration * format.BytesPerSecond);
				Buffer = new byte[len];
			}
			public void FillBufferTo(int pos) {
				Source.FillBuffer(Buffer, LoadPosition, pos - LoadPosition);
				LoadPosition = pos;
			}
		}
		Cache _cache;
		int _pos;

		/// <inheritdoc />
		public override bool EndOfData => _pos >= _cache.Buffer.Length;

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			_cache.Buffer = null;
		}

		/// <inheritdoc />
		protected internal override bool IsFormatSupported(WaveFormat format) {
			return _cache.Source.IsFormatSupported(format);
		}

		/// <inheritdoc />
		protected override void OnSetFormat() {
			base.OnSetFormat();
			if (_cache.Buffer != null) return;
			_cache.SetFormat(Format, BufferSize);
		}

		/// <inheritdoc />
		protected internal override void FillBuffer(byte[] buffer, int offset, int length) {
			int loadTo = Math.Min(_cache.Buffer.Length, _pos + length);
			if (loadTo > _cache.LoadPosition) _cache.FillBufferTo(loadTo);
			int rem = _cache.Buffer.Length - _pos;
			int len = Math.Min(rem, length);
			if (len > 0) {
				Array.Copy(_cache.Buffer, _pos, buffer, offset, len);
				_pos += len;
			}
			SilentBuffer(buffer, offset + len, length - len);
		}
	}
}
