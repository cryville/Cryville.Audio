using System;
using System.IO;
using UnsafeIL;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that caches data for reuse.
	/// </summary>
	/// <remarks>
	/// <para>This stream is not seekable. Use <see cref="Rewind" /> to reset its timestamp to zero.</para>
	/// </remarks>
	public class CachedAudioSource : AudioStream {
		/// <summary>
		/// Creates an instance of the <see cref="CachedAudioSource" /> class.
		/// </summary>
		/// <param name="source">The <see cref="AudioStream" /> to be cached.</param>
		/// <param name="duration">The duration of the cache in seconds.</param>
		public CachedAudioSource(AudioStream source, double duration) {
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
		public void Rewind() => _pos = 0;

		class Cache {
			public int LoadPosition;
			public readonly AudioStream Source;
			public readonly double Duration;
			public byte[] Buffer;
			public Cache(AudioStream source, double duration) {
				Source = source;
				Duration = duration;
			}
			public void SetFormat(WaveFormat format, int bufferSize) {
				Source.SetFormat(format, bufferSize);
				int len = format.Align(Duration * format.BytesPerSecond);
				Buffer = new byte[len];
			}
			public void FillBufferTo(int pos) {
				Source.Read(Buffer, LoadPosition, pos - LoadPosition);
				LoadPosition = pos;
			}
		}
		readonly Cache _cache;
		int _pos;

		/// <inheritdoc />
		public override bool EndOfData => _pos >= _cache.Buffer.Length;

		/// <summary>
		/// Whether this audio stream has been disposed.
		/// </summary>
		public bool Disposed { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			Disposed = true;
		}

		/// <inheritdoc />
		protected internal override bool IsFormatSupported(WaveFormat format) {
			return _cache.Source.IsFormatSupported(format);
		}

		/// <inheritdoc />
		protected override void OnSetFormat() {
			if (_cache.Buffer != null) return;
			_cache.SetFormat(Format, BufferSize);
		}

		/// <inheritdoc />
		public override unsafe int Read(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Disposed) throw new ObjectDisposedException(null);
			count = Format.Align(count, true);
			int loadTo = Math.Min(_cache.Buffer.Length, _pos + count);
			if (loadTo > _cache.LoadPosition) _cache.FillBufferTo(loadTo);
			int rem = _cache.Buffer.Length - _pos;
			int len = Math.Min(rem, count);
			if (len > 0) {
				fixed (byte* sptr = _cache.Buffer, dptr = buffer) {
					Unsafe.CopyBlock(dptr + offset, sptr + _pos, (uint)len);
				}
				_pos += len;
			}
			SilentBuffer(Format, buffer, offset + len, count - len);
			return len;
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => false;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override long Length => _cache.Buffer.Length;
		/// <inheritdoc />
		public override long Position { get => _pos; set => throw new NotSupportedException(); }
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
